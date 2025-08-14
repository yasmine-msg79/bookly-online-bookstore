using BookStore.Repositories;
using BookStore.Models;
using System;

namespace BookStore.Services
{
    public class BookService : IBookService
    {
        private readonly IBookRepository _bookRepo;
        private readonly IStockLogRepository _stockLogRepository;

        public BookService(IBookRepository bookRepo, IStockLogRepository stockLogRepository)
        {
            _bookRepo = bookRepo;
            _stockLogRepository = stockLogRepository;
        }

        public IEnumerable<Book> ListAllBooks()
        {
            return _bookRepo.GetAllBooks();
        }

        public void AddBook(Book book)
        {
            if (_bookRepo.GetByISBN(book.ISBN) != null)
                throw new Exception("ISBN already exists.");

            _bookRepo.AddBook(book);
        }

        // public Book GetBook(string id)
        // {
        //     return _bookRepo.GetByISBN(id);
        // }

        public Book? GetBookById(int id)
        {
            return _bookRepo.GetById(id);
        }

        public void UpdateBook(Book book)
        {
            if (_bookRepo.GetById(book.Id) == null)
                throw new Exception("Book not found.");

            _bookRepo.UpdateBook(book);
        }

        public void DeleteBook(int id)
        {
            if (_bookRepo.GetById(id) == null)
                throw new Exception("Book not found.");

            _bookRepo.DeleteBook(id);
        }

        public void UpdateStock(int bookId, int quantityChange, string reason)
        {
        var book = _bookRepo.GetById(bookId);
        if (book == null)
            throw new Exception("Book not found.");

        book.StockQuantity = quantityChange;

            // Prevent negative stock
            if (book.StockQuantity < 0)
                throw new Exception("Stock cannot be negative.");

            _bookRepo.UpdateBook(book);

            // Log the change
            var log = new StockLog
            {
                BookId = bookId,
                QuantityChanged = quantityChange,
                ChangeDate = DateTime.Now,
                Reason = reason
            };
            _stockLogRepository.AddLog(log);
        }

        public IEnumerable<StockLog> GetStockLogs(int bookId)
        {
            return _stockLogRepository.GetLogsForBook(bookId);
        }

        public bool IsBookInStock(int bookId, int quantityRequired)
        {
            var book = _bookRepo.GetById(bookId);
            return book != null && book.StockQuantity >= quantityRequired;
        }
    }
}
