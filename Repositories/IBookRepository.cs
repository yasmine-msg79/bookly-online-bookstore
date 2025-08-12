using BookStore.Models;
using System.Collections.Generic;

namespace BookStore.Repositories
{
    public interface IBookRepository
    {
        IEnumerable<Book> GetAllBooks();
        Book? GetByISBN(string isbn);

        Book? GetById(int id);

        void AddBook(Book book);
        void UpdateBook(Book book);
        void DeleteBook(int id);
    }
}