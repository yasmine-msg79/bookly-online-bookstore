namespace BookStore.Models
{
    public class Cart
    {
        public int Id { get; set; }
        public string UserId { get; set; }

        public virtual ApplicationUser User { get; set; }
        public virtual ICollection<CartItem> CartItems { get; set; }
    }
}
