using BookStore.Data;
using BookStore.Models;
using System.Collections.Generic;
using System.Linq;

namespace BookStore.Repositories
{
    public class StockLogRepository : IStockLogRepository
    {
        private readonly ApplicationDbContext _context;

        public StockLogRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public void AddLog(StockLog log)
        {
            _context.StockLogs.Add(log);
        }

        public IEnumerable<StockLog> GetLogsForBook(int bookId)
        {
            return _context.StockLogs
                .Where(l => l.BookId == bookId)
                .OrderByDescending(l => l.ChangeDate)
                .ToList();
        }

        public IEnumerable<StockLog> GetAllLogs()
        {
            return _context.StockLogs
                .OrderByDescending(l => l.ChangeDate)
                .ToList();
        }

        public void Save()
        {
            _context.SaveChanges();
        }
    }
}