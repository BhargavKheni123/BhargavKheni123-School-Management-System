using digital.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

public interface ISubCategoryRepository
{
    List<SubCategory> GetAllSubCategories();
    void AddSubCategory(SubCategory subCategory);
    List<SubCategory> GetSubCategoriesWithCategory();
    List<SelectListItem> GetCategorySelectList();
}
