namespace BookStore.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public decimal Price { get; set; }
        public string Status { get; set; }
        public DateTime Created { get; set; }

        public ApplicationUser User { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; }  
    }
}
