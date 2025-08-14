using BookStore.Data;
using BookStore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookStore.Controllers
{
    public class BooksController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BooksController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Books
        public async Task<IActionResult> Index(
            string searchTerm,
            string sortBy,
            int? categoryId,
            decimal? minPrice,
            decimal? maxPrice,
            int page = 1)
        {
            const int PageSize = 9;

            // Store current filter values in ViewBag to persist in the form
            ViewBag.CurrentSearch = searchTerm;
            ViewBag.CurrentCategory = categoryId;
            ViewBag.CurrentMinPrice = minPrice;
            ViewBag.CurrentMaxPrice = maxPrice;
            ViewBag.CurrentSort = sortBy;
            ViewBag.PageNumber = page;

            // Start query
            var books = _context.Books
                .Include(b => b.Category)
                .AsQueryable();

            // Search filter
            if (!string.IsNullOrEmpty(searchTerm))
            {
                books = books.Where(b =>
                    b.Title.Contains(searchTerm) ||
                    b.Author.Contains(searchTerm) ||
                    b.ISBN.Contains(searchTerm));
            }

            // Category filter
            if (categoryId.HasValue)
            {
                books = books.Where(b => b.Category.Id == categoryId.Value);
            }


            // Price filters
            if (minPrice.HasValue)
            {
                books = books.Where(b => b.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                books = books.Where(b => b.Price <= maxPrice.Value);
            }

            // Sorting
            books = sortBy switch
            {
                "title_desc" => books.OrderByDescending(b => b.Title),
                "price_asc" => books.OrderBy(b => b.Price),
                "price_desc" => books.OrderByDescending(b => b.Price),
                _ => books.OrderBy(b => b.Title) // default: title_asc
            };

            // Pagination
            var totalCount = await books.CountAsync();
            var items = await books
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            ViewBag.TotalCount = totalCount;
            ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)PageSize);
            ViewBag.HasPreviousPage = page > 1;
            ViewBag.HasNextPage = page < ViewBag.TotalPages;

            // ✅ Create SelectList for sorting dropdown (this avoids inline C# in option tags)
            ViewBag.SortOptions = new SelectList(
                new[]
                {
                    new { Value = "title_asc", Text = "Title A-Z" },
                    new { Value = "title_desc", Text = "Title Z-A" },
                    new { Value = "price_asc", Text = "Price Low to High" },
                    new { Value = "price_desc", Text = "Price High to Low" }
                },
                "Value",
                "Text",
                sortBy ?? "title_asc" // selected value
            );

            // Load categories for filter dropdown
            ViewBag.Categories = await _context.Categories.ToListAsync();

            return View(items);
        }

        // GET: /Books/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var book = await _context.Books
                .Include(b => b.Category)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }
    }
}