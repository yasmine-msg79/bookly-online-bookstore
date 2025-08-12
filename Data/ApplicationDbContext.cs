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
