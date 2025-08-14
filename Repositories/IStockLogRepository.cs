using BookStore.Models;
using System.Collections.Generic;

namespace BookStore.Repositories
{
    public interface IStockLogRepository
    {
        void AddLog(StockLog log);
        IEnumerable<StockLog> GetLogsForBook(int bookId);
        IEnumerable<StockLog> GetAllLogs();
        void Save();
    }
}