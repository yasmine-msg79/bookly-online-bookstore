using BookStore.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookStore.Models
{
    public class Cart
    {
        public int Id { get; set; }

        public string? UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }

        public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    }
}