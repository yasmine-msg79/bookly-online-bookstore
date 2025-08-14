using Microsoft.AspNetCore.Mvc;
using BookStore.Models;
using BookStore.Services;
using System.Collections.Generic;
using System.Linq;

namespace BookStore.Controllers
{
    public class BookController : Controller
    {
        private readonly IBookService _bookService;
        private readonly ICategoryService _categoryService;

        private static Dictionary<string, int> _stockAdjustments = new();

        public BookController(IBookService bookService, ICategoryService categoryService)
        {
            _bookService = bookService;
            _categoryService = categoryService;
        }

        // Stock Management
        public IActionResult StockManagement()
        {
            var books = _bookService.ListAllBooks();

            var model = books.Select(b =>
            {
                _stockAdjustments.TryGetValue(b.Id.ToString(), out int adj);
                return new BookManagementViewModel
                {
                    Id = b.Id,
                    Title = b.Title,
                    StockQuantity = b.StockQuantity + adj,
                };
            }).ToList();

            return View(model);
        }

        [HttpPost]
        public IActionResult AdjustStock(string bookId, int adjustment)
        {
            if (_stockAdjustments.ContainsKey(bookId))
                _stockAdjustments[bookId] += adjustment;
            else
                _stockAdjustments[bookId] = adjustment;

            return RedirectToAction("StockManagement");
        }

        [HttpPost]
        public IActionResult ConfirmStock(List<BookManagementViewModel> model)
        {
            foreach (var item in model)
            {
            
                var book = _bookService.GetBookById(item.Id);
                if (book != null)
                {
                    book.StockQuantity = item.StockQuantity;
                    _bookService.UpdateStock(book.Id, item.StockQuantity, "Stock adjustment from management");
                }
            }

            TempData["SuccessMessage"] = "Stock updated successfully!";
            return RedirectToAction("StockManagement");
        }

        // Book & Category Management

        public IActionResult BookAndCategoryManagement()
        {
            var books = _bookService.ListAllBooks();
            var categories = _categoryService.GetAllCategories();

            var model = new BookAndCategoryViewModel
            {
                Books = books.Select(b => new BookManagementViewModel
                {
                    Id = b.Id,
                    Title = b.Title,
                    Author = b.Author,
                    ISBN = b.ISBN,
                    Price = b.Price,
                    Description = b.Description,
                    StockQuantity = b.StockQuantity,
                    CategoryId = _categoryService.GetCategoryById(b.category?.Id ?? 0)?.Id ?? 0,
                    CategoryName = _categoryService.GetCategoryById(b.category?.Id ?? 0)?.Name ?? "No Category"
                }).ToList(),  
                Categories = categories
            };

            return View(model);
        }

        // Book Management

        [HttpGet]
        public IActionResult GetBook(int id)
        {
            var book = _bookService.GetBookById(id);
            if (book == null) return Json(null);

            return Json(new
            {
                id = book.Id,
                title = book.Title,
                author = book.Author,
                isbn = book.ISBN,
                price = book.Price,
                description = book.Description,
                categoryId = book.category?.Id ?? 0
            });
        }

        [HttpPost]
        public IActionResult AddBook(BookManagementViewModel book)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });

            _bookService.AddBook(new Book
            {
                Title = book.Title,
                Author = book.Author,
                ISBN = book.ISBN,
                Price = book.Price,
                Description = book.Description,
                StockQuantity = book.StockQuantity,
                category = _categoryService.GetCategoryById(book.CategoryId)
            });

            TempData["AddMessage"] = "Book added successfully!";
            return RedirectToAction("BookAndCategoryManagement");
        }

        [HttpPost]
        public IActionResult UpdateBook(BookManagementViewModel model)
        {
            if (!ModelState.IsValid) return Json(new { success = false });

            var book = _bookService.GetBookById(model.Id);
            if (book == null) return Json(new { success = false, message = "Book not found" });

            book.Title = model.Title;
            book.Author = model.Author;
            book.ISBN = model.ISBN;
            book.Description = model.Description;
            book.Price = model.Price;
            book.category = _categoryService.GetCategoryById(model.CategoryId);

            _bookService.UpdateBook(book);
            TempData["UpdateMessage"] = "Book updated successfully!";
            return RedirectToAction("BookAndCategoryManagement");
        }

        [HttpPost]
        public IActionResult DeleteBook(int id)
        {
            var book = _bookService.GetBookById(id);
            if (book == null) return NotFound();
            _bookService.DeleteBook(id);
            TempData["DeleteMessage"] = "Book deleted successfully!";
            return RedirectToAction("BookAndCategoryManagement");
        }
      
        // CATEGORY MANAGEMENT

        [HttpGet]
        public IActionResult GetCategory(int id)
        {
            var cat = _categoryService.GetCategoryById(id);
            if (cat == null) return Json(null);
            return Json(new { Id = cat.Id, Name = cat.Name });
        }

        [HttpPost]
        public IActionResult AddCategory(Category category)
        {
            if (!ModelState.IsValid)
                return RedirectToAction("BookAndCategoryManagement");

            _categoryService.AddCategory(category);
            TempData["AddMessage"] = "Category added successfully!";
            return RedirectToAction("BookAndCategoryManagement");
        }

        [HttpPost]
        public IActionResult UpdateCategory(Category category)
        {
            if (!ModelState.IsValid)
                return RedirectToAction("BookAndCategoryManagement");

            _categoryService.UpdateCategory(category);
            TempData["UpdateMessage"] = "Category updated successfully!";
            return RedirectToAction("BookAndCategoryManagement");
        }

        [HttpPost]
        public IActionResult DeleteCategory(int id)
        {
            var cat = _categoryService.GetCategoryById(id);
            if (cat == null) return Json(new { success = false, message = "Category not found" });

            _categoryService.DeleteCategory(id);
            TempData["DeleteMessage"] = "Category deleted successfully!";
            return RedirectToAction("BookAndCategoryManagement");
        }



    } 
}
