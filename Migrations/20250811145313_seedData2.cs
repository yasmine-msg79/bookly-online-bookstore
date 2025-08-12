using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookStore.Migrations
{
    /// <inheritdoc />
    public partial class seedData2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.InsertData(
                table: "Books",
                columns: new[] { "Id", "Author", "Description", "ISBN", "ImageUrl", "Price", "StockQuantity", "Title" },
                values: new object[] { 2, "F. Scott Fitzgerald", "A classic novel of the Jazz Age.", "1234567890123", "/images/The-Great-Gatsby.jpg", 19.99m, 10, "The Great Gatsby" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.InsertData(
                table: "Books",
                columns: new[] { "Id", "Author", "Description", "ISBN", "ImageUrl", "Price", "StockQuantity", "Title" },
                values: new object[] { 1, "F. Scott Fitzgerald", "A classic novel of the Jazz Age.", "1234567890123", "/images/The-Great-Gatsby.jpg", 19.99m, 10, "The Great Gatsby" });
        }
    }
}
