using AutoMapper;
using digital.Interfaces;
using digital.Models;
using digital.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace digital.Controllers
{
    public class SubCategoryController : Controller
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly ISubCategoryRepository _subCategoryRepository;
        private readonly IGenericRepository<SubCategory> _subCategoryRepo;
        private readonly IMapper _mapper;

        public SubCategoryController(
            ICategoryRepository categoryRepository,
            ISubCategoryRepository subCategoryRepository,
            IGenericRepository<SubCategory> subCategoryRepo,
            IMapper mapper)
        {
            _categoryRepository = categoryRepository;
            _subCategoryRepository = subCategoryRepository;
            _subCategoryRepo = subCategoryRepo;
            _mapper = mapper;
        }

        [HttpGet]
        public IActionResult Subcategories()
        {
            var subCategories = _subCategoryRepository.GetSubCategoriesWithCategory();
            var viewModel = new SubCategoryViewModel
            {
                Categories = _categoryRepository.GetCategorySelectList(),
                SubCategoryList = subCategories
            };
            return View(viewModel);
        }

        [HttpPost]
        public IActionResult Subcategories(SubCategoryViewModel model)
        {
            if (!string.IsNullOrEmpty(model.SubCategoryToEdit?.Name) && model.SubCategoryToEdit.CategoryId > 0)
            {
                var newSubCategory = _mapper.Map<SubCategory>(model.SubCategoryToEdit);
                _subCategoryRepository.AddSubCategory(newSubCategory);
            }

            var viewModel = new SubCategoryViewModel
            {
                Categories = _categoryRepository.GetCategorySelectList(),
                SubCategoryList = _subCategoryRepository.GetSubCategoriesWithCategory()
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> EditSub(int id)
        {
            var subCategory = await _subCategoryRepo.GetByIdAsync(id);
            if (subCategory == null)
                return NotFound();

            var viewModel = new SubCategoryViewModel
            {
                Categories = _categoryRepository.GetCategorySelectList(),
                SubCategoryToEdit = _mapper.Map<SubCategory>(subCategory)
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> EditSub(SubCategoryViewModel model)
        {
            if (ModelState.IsValid)
            {
                var updatedSub = _mapper.Map<SubCategory>(model.SubCategoryToEdit);
                updatedSub.CreatedDate = DateTime.Now;
                updatedSub.CreatedBy = "admin";

                await _subCategoryRepo.UpdateAsync(updatedSub);
                await _subCategoryRepo.SaveAsync();

                return RedirectToAction("Subcategories");
            }

            model.Categories = _categoryRepository.GetCategorySelectList();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateSub(int id, int CategoryId, string Name)
        {
            var sub = await _subCategoryRepo.GetByIdAsync(id);
            if (sub != null)
            {
                sub.CategoryId = CategoryId;
                sub.Name = Name;
                sub.CreatedDate = DateTime.Now;
                sub.CreatedBy = "admin";

                await _subCategoryRepo.UpdateAsync(sub);
                await _subCategoryRepo.SaveAsync();
            }

            return RedirectToAction("Subcategories");
        }

        [HttpGet]
        public async Task<IActionResult> DeleteSub(int id)
        {
            var sub = await _subCategoryRepo.GetByIdAsync(id);
            if (sub != null)
            {
                await _subCategoryRepo.DeleteAsync(sub);
                await _subCategoryRepo.SaveAsync();
            }

            return RedirectToAction("Subcategories");
        }

        // JSON: Get subcategories by CategoryId (used in Student form)
        [HttpGet]
        public JsonResult GetSubCategories(int categoryId)
        {
            // NOTE: repository returns SelectList, but we just need id/name here
            var list = _subCategoryRepository
                .GetSubCategoriesWithCategory()
                .Where(sc => sc.CategoryId == categoryId)
                .Select(x => new { id = x.Id, name = x.Name })
                .ToList();
            return Json(list);
        }
    }
}
