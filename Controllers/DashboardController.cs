using BookStore.Data;
using BookStore.Models;
using BookStore.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace BookStore.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Dashboard()
        {
            var viewModel = new AdminDashboardViewModel();

            // Total books count
            viewModel.TotalBooks = await _context.Books.CountAsync();

            // Total stock quantity (sum all book stock)
            viewModel.TotalStock = await _context.Books.SumAsync(b => b.StockQuantity);

            // Recent orders count (e.g. orders in last 30 days)
            var recentDate = System.DateTime.UtcNow.AddDays(-30);
            viewModel.RecentOrdersCount = await _context.Orders
                .Where(o => o.Created >= recentDate)
                .CountAsync();

            // Top-selling books: group order items by book, sum quantity sold, order desc, take top 5
            var topSellingBooks = await _context.OrderItems
                .GroupBy(oi => oi.BookId)
                .Select(g => new
                {
                    BookId = g.Key,
                    TotalSold = g.Sum(oi => oi.Quantity)
                })
                .OrderByDescending(g => g.TotalSold)
                .Take(5)
                .ToListAsync();

            // Build detailed info for top selling books
            var bookIds = topSellingBooks.Select(b => b.BookId).ToList();

            var booksDetails = await _context.Books
                .Where(b => bookIds.Contains(b.Id))
                .Include(b => b.StockLogs)
                .Include(b => b.Reviews)
                .ToListAsync();

            viewModel.TopSellingBooks = topSellingBooks.Select(b =>
            {
                var book = booksDetails.First(x => x.Id == b.BookId);
                double avgRating = 0;
                if (book.Reviews.Any())
                    avgRating = book.Reviews.Average(r => r.Rating);

                return new TopSellingBookViewModel
                {
                    BookId = book.Id,
                    Title = book.Title,
                    TotalSold = b.TotalSold,
                    Author = book.Author,
                    StockQuantity = book.StockQuantity,
                    AverageRating = avgRating,
                    StockLogs = book.StockLogs.Select(sl => new StockLogViewModel
                    {
                        ChangeDate = sl.ChangeDate,
                        QuantityChanged = sl.QuantityChanged,
                        Reason = sl.Reason
                    }).OrderByDescending(sl => sl.ChangeDate).ToList()
                };
            }).ToList();

            var allBooks = await _context.Books
                .Include(b => b.Reviews)
                .Include(b => b.OrderItems)
                .ToListAsync();
            viewModel.AllBooks = allBooks.Select(book =>
            {
                var totalSold = book.OrderItems?.Sum(oi => oi.Quantity) ?? 0;
                var avgRating = (book.Reviews != null && book.Reviews.Any()) ? book.Reviews.Average(r => r.Rating) : 0;

                return new TopSellingBookViewModel
                {
                    BookId = book.Id,
                    Title = book.Title,
                    Author = book.Author,
                    StockQuantity = book.StockQuantity,
                    TotalSold = totalSold,
                    AverageRating = avgRating,
                    StockLogs = (book.StockLogs != null) ?
                        book.StockLogs.Select(sl => new StockLogViewModel
                        {
                            ChangeDate = sl.ChangeDate,
                            QuantityChanged = sl.QuantityChanged,
                            Reason = sl.Reason
                        }).OrderByDescending(sl => sl.ChangeDate).ToList()
                        : new List<StockLogViewModel>()
                };
            }).ToList();


            viewModel.Users = await _context.Users
                .Where(u => u.Role != Role.SuperAdmin)  
                .Select(u => new ApplicationUserViewModel
                {
                    Id = u.Id,
                    FullName = u.FirstName + " " + u.LastName,
                    Email = u.Email,
                    Role = u.Role.ToString(),
                    IsSuspended = u.IsSuspended // Implement suspension logic if you have it
                })
                .ToListAsync();

            // Categories with count of books in each
            viewModel.Categories = await _context.Categories
                .Select(c => new CategoryViewModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    BookCount = c.Books.Count()
                })
                .ToListAsync();

            return View(viewModel);
        }
    }
}
