using AutoMapper;
using digital.Interfaces;
using digital.Models;
using digital.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace digital.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IGenericRepository<Category> _categoryRepo;
        private readonly IMapper _mapper;

        public CategoryController(
            ICategoryRepository categoryRepository,
            IGenericRepository<Category> categoryRepo,
            IMapper mapper)
        {
            _categoryRepository = categoryRepository;
            _categoryRepo = categoryRepo;
            _mapper = mapper;
        }

        
        [HttpGet]
        public IActionResult Category()
        {
            var categories = _categoryRepository.GetAllCategories();
            var viewModelList = _mapper.Map<List<CategoryViewModel>>(categories);
            return View(viewModelList);
        }

        
        [HttpPost]
        public IActionResult Category(CategoryViewModel model)
        {
            if (ModelState.IsValid)
            {
                var category = _mapper.Map<Category>(model);
                _categoryRepository.AddCategory(category);
                return RedirectToAction("Category");
            }

            var categories = _categoryRepository.GetAllCategories();
            var viewModelList = _mapper.Map<List<CategoryViewModel>>(categories);
            return View(viewModelList);
        }

        [HttpGet]
        public async Task<IActionResult> EditCategory(int id)
        {
            var category = await _categoryRepo.GetByIdAsync(id);
            if (category == null) return NotFound();

            var viewModel = _mapper.Map<CategoryViewModel>(category);
            return View("EditCategory", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> EditCategory(CategoryViewModel model)
        {
            if (ModelState.IsValid)
            {
                var existingCategory = await _categoryRepo.GetByIdAsync(model.Id);
                if (existingCategory == null) return NotFound();

                existingCategory.Name = model.Name;

                await _categoryRepo.UpdateAsync(existingCategory);
                await _categoryRepo.SaveAsync();

                return RedirectToAction("Category");
            }

            return View("EditCategory", model);
        }


        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _categoryRepo.GetByIdAsync(id);
            if (category != null)
            {
                await _categoryRepo.DeleteAsync(category);
                await _categoryRepo.SaveAsync();
            }
            return RedirectToAction("Category");
        }
    }
}
