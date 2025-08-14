using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BookStore.Models
{
    public class Book
    {
        public int Id { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string Author { get; set; }
        public string Description { get; set; }
        [Required]
        [StringLength(15)]
        public string ISBN { get; set; }
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }

        public int StockQuantity { get; set; }

        public virtual ICollection<BookReview> Reviews { get; set; }
        public virtual ICollection<Category>? Categories { get; set; }
        public virtual ICollection<OrderItem> OrderItems { get; set; }
        public virtual ICollection<StockLog> StockLogs { get; set; }
        public virtual Category? category {get; set;}
    }
}
