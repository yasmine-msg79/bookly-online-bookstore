using Microsoft.AspNetCore.Mvc;
using BookStore.Models;
using BookStore.Services;

namespace BookStore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookController : ControllerBase
    {
        private readonly IBookService _bookService;
        private readonly ICategoryService _categoryService;

        public BookController(IBookService bookService, ICategoryService categoryService)
        {
            _bookService = bookService;
            _categoryService = categoryService;
        }

        // book management

        [HttpGet("books")]
        public IActionResult GetAllBooks()
        {
            var books = _bookService.ListAllBooks();
            return Ok(books);
        }

        [HttpGet("books/{id}")]
        public IActionResult GetBookById(int id)
        {
            var book = _bookService.GetBookById(id);
            if (book == null)
                return NotFound($"Book with ID {id} not found.");
            return Ok(book);
        }

        [HttpPost("books")]
        public IActionResult AddBook([FromBody] Book book)
        {
            if (book == null)
                return BadRequest("Book data is required.");
            
            _bookService.AddBook(book);
            return CreatedAtAction(nameof(GetBookById), new { id = book.Id }, book);
        }

        [HttpPut("books/{id}")]
        public IActionResult UpdateBook(int id, [FromBody] Book book)
        {
            if (book == null || id != book.Id)
                return BadRequest("Invalid book data.");

            var existingBook = _bookService.GetBookById(id);
            if (existingBook == null)
                return NotFound($"Book with ID {id} not found.");

            _bookService.UpdateBook(book);
            return NoContent();
        }

        [HttpDelete("books/{id}")]
        public IActionResult DeleteBook(int id)
        {
            var book = _bookService.GetBookById(id);
            if (book == null)
                return NotFound($"Book with ID {id} not found.");

            _bookService.DeleteBook(id);
            return NoContent();
        }

        // category management

        [HttpGet("categories")]
        public IActionResult GetAllCategories()
        {
            var categories = _categoryService.GetAllCategories();
            return Ok(categories);
        }

        [HttpPost("categories")]
        public IActionResult AddCategory([FromBody] Category category)
        {
            if (category == null)
                return BadRequest("Category data is required.");

            _categoryService.AddCategory(category);
            return Ok(category);
        }

        [HttpPut("categories/{id}")]
        public IActionResult UpdateCategory(int id, [FromBody] Category category)
        {
            if (category == null || id != category.Id)
                return BadRequest("Invalid category data.");

            var existingCategory = _categoryService.GetCategoryById(id);
            if (existingCategory == null)
                return NotFound($"Category with ID {id} not found.");

            _categoryService.UpdateCategory(category);
            return NoContent();
        }

        [HttpDelete("categories/{id}")]
        public IActionResult DeleteCategory(int id)
        {
            var category = _categoryService.GetCategoryById(id);
            if (category == null)
                return NotFound($"Category with ID {id} not found.");

            _categoryService.DeleteCategory(id);
            return NoContent();
        }

        // stock management

        [HttpPut("books/{id}/stock/add")]
        public IActionResult AddStock(int id, [FromQuery] int quantity, [FromQuery] string reason = "Stock Increase")
        {
            if (quantity <= 0)
                return BadRequest("Quantity must be greater than zero.");

            _bookService.UpdateStock(id, quantity, reason);
            return Ok("Stock updated successfully.");
        }

        [HttpPut("books/{id}/stock/reduce")]
        public IActionResult ReduceStock(int id, [FromQuery] int quantity, [FromQuery] string reason = "Stock Reduction")
        {
            if (quantity <= 0)
                return BadRequest("Quantity must be greater than zero.");

            _bookService.UpdateStock(id, -quantity, reason);
            // If you need to check for failure, update the service to return a bool and handle accordingly.
            return Ok("Stock reduced successfully.");
        }

        [HttpGet("books/{id}/stock-log")]
        public IActionResult GetStockLog(int id)
        {
            var logs = _bookService.GetStockLogs(id);
            return Ok(logs);
        }
    }
}
