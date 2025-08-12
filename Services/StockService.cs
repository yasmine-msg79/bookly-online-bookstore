// StockService.cs
using BookStore.Repositories;
using BookStore.Models;
using System;
using System.Collections.Generic;

namespace BookStore.Services
{
    public class StockService : IStockService
    {
        private readonly IBookRepository _bookRepo;
        private readonly IStockLogRepository _logRepo;

        public StockService(IBookRepository bookRepo, IStockLogRepository logRepo)
        {
            _bookRepo = bookRepo;
            _logRepo = logRepo;
        }

        public bool UpdateStock(int bookId, int quantityChange, string reason)
        {
            var book = _bookRepo.GetById(bookId);
            if (book == null) return false;

            int newQuantity = book.StockQuantity + quantityChange;
            if (newQuantity < 0) return false; // Prevent negative stock

            book.StockQuantity = newQuantity;
            _bookRepo.UpdateBook(book);

            var log = new StockLog
            {
                BookId = bookId,
                QuantityChanged = quantityChange,
                ChangeDate = DateTime.Now,
                Reason = reason
            };
            _logRepo.AddLog(log);

            _logRepo.Save();

            return true;
        }

        public IEnumerable<StockLog> GetStockLogs(int bookId)
        {
            return _logRepo.GetLogsForBook(bookId);
        }

        public bool IsInStock(int bookId)
        {
            var book = _bookRepo.GetById(bookId);
            return book != null && book.StockQuantity > 0;
        }
    }
}
