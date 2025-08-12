using System.Collections.Generic;
using BookStore.Models;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;

    public CategoryService(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public IEnumerable<Category> GetAllCategories()
    {
        return _categoryRepository.GetAll();
    }

    public Category GetCategoryById(int id)
    {
        return _categoryRepository.GetById(id);
    }

    public void AddCategory(Category category)
    {
        _categoryRepository.Add(category);
    }

    public void UpdateCategory(Category category)
    {
        _categoryRepository.Update(category);
    }

    public void DeleteCategory(int id)
    {
        _categoryRepository.Delete(id);
    }
}
