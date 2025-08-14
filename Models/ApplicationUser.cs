using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace BookStore.Models
{
    public enum Role { SuperAdmin, Admin, Guest}
    public enum Gender { Male, Female}
    public class ApplicationUser : IdentityUser
    {
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        public string? ProfileImageURL { get; set; }
        public Role Role { get; set; }
        public Gender Gender { get; set; }
        public bool IsSuspended { get; set; } = false;
        
        public virtual Cart Cart { get; set; }
        public virtual ICollection<Order> Orders { get; set; }
        public virtual ICollection<BookReview> Reviews { get; set; }
    }
}
