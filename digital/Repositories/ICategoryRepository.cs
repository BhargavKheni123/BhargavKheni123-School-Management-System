using digital.Models;

using Microsoft.AspNetCore.Mvc.Rendering;

public interface ICategoryRepository
{
    List<Category> GetAllCategories();
    void AddCategory(Category category);
    List<SelectListItem> GetCategorySelectList();
}
namespace digital.Repositories
{
    public class ICategoryRepository
    {
    }
}
