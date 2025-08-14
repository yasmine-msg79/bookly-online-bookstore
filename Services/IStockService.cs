using BookStore.Models;
using System.Collections.Generic;

namespace BookStore.Services
{
    public interface IStockService
    {
        bool UpdateStock(int bookId, int quantityChange, string reason);
        IEnumerable<StockLog> GetStockLogs(int bookId);
        bool IsInStock(int bookId);
    }
}