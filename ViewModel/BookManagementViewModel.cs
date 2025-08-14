using System.ComponentModel.DataAnnotations;
using BookStore.Models;

public class BookManagementViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Title is required")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Author is required")]
    public string Author { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "ISBN is required")]
    [StringLength(15, ErrorMessage = "ISBN cannot be longer than 15 characters")]
    public string ISBN { get; set; } = string.Empty;

    public int StockQuantity { get; set; }

    public decimal Price { get; set; }

    [Required(ErrorMessage = "Category is required")]
    public int CategoryId { get; set; }

    public string CategoryName { get; set; } = string.Empty;
}