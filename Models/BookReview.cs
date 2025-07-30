using System.ComponentModel.DataAnnotations;

namespace BookStore.Models
{
    public class BookReview
    {
        public int Id { get; set; }
        public int BookId { get; set; }
        public string UserId { get; set; }
        public string ReviewText { get; set; }
        [Range(1, 5)]
        public int Rating { get; set; }
        public DateTime ReviewDate { get; set; }

        public virtual ApplicationUser User { get; set; }
        public virtual Book Book { get; set; }
    }
}
