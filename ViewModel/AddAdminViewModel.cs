using BookStore.Models;
using System.ComponentModel.DataAnnotations;

public class AddAdminViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    [DataType(DataType.Password)]
    [MinLength(6)]
    public string Password { get; set; }

    [Required]
    public string FirstName { get; set; }

    [Required]
    public string LastName { get; set; }
    [Required]
    public string UserName { get; set; }

    [Required]
    public Gender Gender { get; set; }

    [Phone]
    public string PhoneNumber { get; set; }
}
