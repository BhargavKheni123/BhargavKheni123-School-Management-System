using AutoMapper;
using digital.Interfaces;
using digital.Models;
using digital.ViewModels;
using Microsoft.AspNetCore.Mvc;
using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.IO;

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

        [HttpGet]
        public IActionResult ExportCategoriesToExcel()
        {
            var categories = _categoryRepository.GetAllCategories();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Categories");

            
            worksheet.Cell(1, 1).Value = "No.";
            worksheet.Cell(1, 2).Value = "Standard";
            worksheet.Cell(1, 3).Value = "Created Date";

            int row = 2, counter = 1;

            foreach (var c in categories)
            {
                worksheet.Cell(row, 1).Value = counter++;
                worksheet.Cell(row, 2).Value = c.Name;
                worksheet.Cell(row, 3).Value = c.CreatedDate.ToString("yyyy-MM-dd");
                row++;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var content = stream.ToArray();

            return File(content,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "Categories.xlsx");
        }

        [HttpGet]
        public IActionResult ExportCategoriesToPdf()
        {
            var categories = _categoryRepository.GetAllCategories();
            int counter = 1;
            var fileName = $"Categories_{DateTime.Now:yyyyMMdd_HHmm}.pdf";

            var pdfBytes = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(20);

                    
                    page.Header().Column(col =>
                    {
                        col.Spacing(5);
                        col.Item().AlignCenter().Text("Standard Report")
                            .FontSize(18).SemiBold();
                        col.Item().LineHorizontal(1);
                    });

                    
                    page.Content().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(1); 
                            columns.RelativeColumn(3); 
                            columns.RelativeColumn(3); 
                        });

                        void HeaderCell(string text) =>
                            table.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text(text).SemiBold();

                        HeaderCell("No.");
                        HeaderCell("Standard");
                        HeaderCell("Created Date");

                        foreach (var c in categories)
                        {
                            table.Cell().Padding(3).Text(counter++.ToString());
                            table.Cell().Padding(3).Text(c.Name ?? "");
                            table.Cell().Padding(3).Text(c.CreatedDate.ToString("yyyy-MM-dd"));
                        }
                    });

                    
                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Page ");
                        x.CurrentPageNumber();
                        x.Span(" of ");
                        x.TotalPages();
                    });
                });
            }).GeneratePdf();

            return File(pdfBytes, "application/pdf", fileName);
        }


    }
}
