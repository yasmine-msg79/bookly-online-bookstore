using BookStore.Models;

namespace BookStore.ViewModel
{
    public class AdminDashboardViewModel
    {
        public int TotalBooks { get; set; }
        public int TotalStock { get; set; }
        public int RecentOrdersCount { get; set; }

        public List<TopSellingBookViewModel> TopSellingBooks { get; set; }
        public List<ApplicationUserViewModel> Users { get; set; }
        public List<TopSellingBookViewModel> AllBooks { get; set; }
        public List<CategoryViewModel> Categories { get; set; }
    }

    public class TopSellingBookViewModel
    {
        public int BookId { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public int TotalSold { get; set; }
        public int StockQuantity { get; set; }
        public double AverageRating { get; set; }
        public List<StockLogViewModel> StockLogs { get; set; }
    }

    public class StockLogViewModel
    {
        public DateTime ChangeDate { get; set; }
        public int QuantityChanged { get; set; }
        public string Reason { get; set; }
    }

    public class ApplicationUserViewModel
    {
        public string Id { get; set; }
        public string FullName { get; set; }  // FirstName + LastName
        public string Email { get; set; }
        public string Role { get; set; }
        public bool IsSuspended { get; set; }  // You can track suspension status if you implement it
    }

    public class CategoryViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int BookCount { get; set; }
    }
}
