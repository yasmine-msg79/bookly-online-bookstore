using BookStore.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            
            // Book configurations
            builder.Entity<Book>().HasIndex(b => b.ISBN).IsUnique();

            // many-to-many (Book ↔ Category)
            builder.Entity<Book>().HasMany(b => b.Categories).WithMany(c => c.Books);

            // Cart relationships
            builder.Entity<Cart>()
                .HasOne(c => c.User)
                .WithOne(u => u.Cart)
                .HasForeignKey<Cart>(c => c.UserId)
                .IsRequired(false);

            builder.Entity<CartItem>()
                .HasOne(ci => ci.Cart)
                .WithMany(c => c.CartItems)
                .HasForeignKey(ci => ci.CartId);

            builder.Entity<CartItem>()
                .HasOne(ci => ci.Book)
                .WithMany()
                .HasForeignKey(ci => ci.BookId);

            // Order relationships
            builder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserId)
                .IsRequired(false);

            builder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId);

            builder.Entity<OrderItem>()
                .HasOne(oi => oi.Book)
                .WithMany()
                .HasForeignKey(oi => oi.BookId);

            // StockLog relationships
            builder.Entity<StockLog>()
                .HasOne(sl => sl.Book)
                .WithMany(b => b.StockLogs)
                .HasForeignKey(sl => sl.BookId);

            builder.Entity<StockLog>()
                .HasOne(sl => sl.Order)
                .WithMany(o => o.StockLogs)
                .HasForeignKey(sl => sl.OrderId)
                .IsRequired(false);

            // BookReview relationships
            builder.Entity<BookReview>()
                .HasOne(br => br.User)
                .WithMany(u => u.Reviews)
                .HasForeignKey(br => br.UserId);

            builder.Entity<BookReview>()
                .HasOne(br => br.Book)
                .WithMany(b => b.Reviews)
                .HasForeignKey(br => br.BookId);

            // Ensure email is unique
            builder.Entity<ApplicationUser>()
                .HasIndex(u => u.Email)
                .IsUnique();

            builder.Entity<Book>().HasData(
            new Book
            {
                Id = 2,
                Title = "The Great Gatsby",
                Author = "F. Scott Fitzgerald",
                ISBN = "1234567890123",
                Price = 19.99m,
                StockQuantity = 10,
                Description = "A classic novel of the Jazz Age.",
                ImageUrl = "/images/The-Great-Gatsby.jpg"
            },
            new Book
            {
                Id = 3,
                Title = "1984",
                Author = "George Orwell",
                ISBN = "2345678901234",
                Price = 15.99m,
                StockQuantity = 7,
                Description = "A dystopian novel about totalitarianism.",
                ImageUrl = "/images/1984.jpg"
            },
            new Book
            {
                Id = 4,
                Title = "To Kill a Mockingbird",
                Author = "Harper Lee",
                ISBN = "3456789012345",
                Price = 18.50m,
                StockQuantity = 12,
                Description = "A novel about racial injustice in the Deep South.",
                ImageUrl = "/images/To-Kill-a-Mockingbird.jpg"
            },
            new Book
            {
                Id = 5,
                Title = "Pride and Prejudice",
                Author = "Jane Austen",
                ISBN = "4567890123456",
                Price = 14.99m,
                StockQuantity = 15,
                Description = "A romantic novel about manners and marriage.",
                ImageUrl = "/images/Pride-and-Prejudice.jpeg"
            },
            new Book
            {
                Id = 6,
                Title = "The Catcher in the Rye",
                Author = "J.D. Salinger",
                ISBN = "5678901234567",
                Price = 16.99m,
                StockQuantity = 9,
                Description = "A story about teenage rebellion and angst.",
                ImageUrl = "/images/The-Catcher-in-the-Rye.jpeg"
            });

        }

        public DbSet<Book> Books { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<BookReview> BookReviews { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }    
        public DbSet<StockLog> StockLogs { get; set; }
    }
}
