using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BookStore.Migrations
{
    /// <inheritdoc />
    public partial class seedData3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Books",
                columns: new[] { "Id", "Author", "Description", "ISBN", "ImageUrl", "Price", "StockQuantity", "Title" },
                values: new object[,]
                {
                    { 3, "George Orwell", "A dystopian novel about totalitarianism.", "2345678901234", "/images/1984.jpg", 15.99m, 7, "1984" },
                    { 4, "Harper Lee", "A novel about racial injustice in the Deep South.", "3456789012345", "/images/To-Kill-a-Mockingbird.jpg", 18.50m, 12, "To Kill a Mockingbird" },
                    { 5, "Jane Austen", "A romantic novel about manners and marriage.", "4567890123456", "/images/Pride-and-Prejudice.jpg", 14.99m, 15, "Pride and Prejudice" },
                    { 6, "J.D. Salinger", "A story about teenage rebellion and angst.", "5678901234567", "/images/The-Catcher-in-the-Rye.jpg", 16.99m, 9, "The Catcher in the Rye" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 6);
        }
    }
}
