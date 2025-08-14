namespace BookStore.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime Created { get; set; }

        // Shipping information
        public string ShippingFirstName { get; set; }
        public string ShippingLastName { get; set; }
        public string ShippingEmail { get; set; }
        public string ShippingPhone { get; set; }
        public string ShippingAddress { get; set; }
        public string ShippingCity { get; set; }
        public string ShippingState { get; set; }
        public string ShippingZipCode { get; set; }
        public string ShippingCountry { get; set; }

        public virtual ApplicationUser User { get; set; }
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public virtual ICollection<StockLog> StockLogs { get; set; } = new List<StockLog>();
    }
}