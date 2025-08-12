using System.Collections.Generic;
using BookStore.Models;

namespace BookStore.Services
{
    public interface IBookService
    {
        IEnumerable<Book> ListAllBooks();
        Book GetBook(string id);
        Book? GetBookById(int id);
        void AddBook(Book book);
        void UpdateBook(Book book);
        void DeleteBook(int id);
        void UpdateStock(int bookId, int quantityChange, string reason);
        IEnumerable<StockLog> GetStockLogs(int bookId);
        bool IsBookInStock(int bookId, int quantityRequired);
    }
}