using digital.Models;


using Microsoft.AspNetCore.Mvc.Rendering;

public class CategoryRepository : ICategoryRepository
{
    private readonly ApplicationDbContext _context;

    public CategoryRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public List<Category> GetAllCategories()
    {
        return _context.Categories.ToList();
    }

    public void AddCategory(Category category)
    {
        category.CreatedDate = DateTime.Now;
        _context.Categories.Add(category);
        _context.SaveChanges();
    }

    public List<SelectListItem> GetCategorySelectList()
    {
        return _context.Categories
            .Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name
            }).ToList();
    }
}
