using BookStore.Models;

public class BookAndCategoryViewModel
    {
    public IEnumerable<BookManagementViewModel> Books { get; set; } = new List<BookManagementViewModel>();
        public IEnumerable<Category> Categories { get; set; } = new List<Category>();
    }