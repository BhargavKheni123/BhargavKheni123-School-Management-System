using digital.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

public class SubCategoryRepository : ISubCategoryRepository
{
    private readonly ApplicationDbContext _context;

    public SubCategoryRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public List<SubCategory> GetAllSubCategories()
    {
        return _context.SubCategories.ToList();
    }

    public List<SubCategory> GetSubCategoriesWithCategory()
    {
        return _context.SubCategories.Include(s => s.Category).ToList();
    }

    public void AddSubCategory(SubCategory subCategory)
    {
        subCategory.CreatedDate = DateTime.Now;
        subCategory.CreatedBy = "admin"; // or fetch from session
        _context.SubCategories.Add(subCategory);
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
