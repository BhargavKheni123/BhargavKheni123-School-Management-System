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
                newSubCategory.CreatedBy = "admin";
                newSubCategory.CreatedDate = DateTime.Now;

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
            if (ModelState.IsValid && model.SubCategoryToEdit != null)
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

        
        [HttpGet]
        public JsonResult GetSubCategories(int categoryId)
        {
            var list = _subCategoryRepository
                .GetSubCategoriesWithCategory()
                .Where(sc => sc.CategoryId == categoryId)
                .Select(x => new { id = x.Id, name = x.Name })
                .ToList();
            return Json(list);
        }

        [HttpGet]
        public IActionResult ExportSubCategoriesToExcel()
        {
            var subCategories = _subCategoryRepository.GetSubCategoriesWithCategory();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("SubCategories");

           
            worksheet.Cell(1, 1).Value = "No.";
            worksheet.Cell(1, 2).Value = "Standard";
            worksheet.Cell(1, 3).Value = "Class";

            int row = 2, counter = 1;

            foreach (var sc in subCategories)
            {
                worksheet.Cell(row, 1).Value = counter++;
                worksheet.Cell(row, 2).Value = sc.Category?.Name ?? "";
                worksheet.Cell(row, 3).Value = sc.Name ?? "";
                row++;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var content = stream.ToArray();

            return File(content,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "SubCategories.xlsx");
        }

        [HttpGet]
        public IActionResult ExportSubCategoriesToPdf()
        {
            var subCategories = _subCategoryRepository.GetSubCategoriesWithCategory();
            int counter = 1;
            var fileName = $"SubCategories_{DateTime.Now:yyyyMMdd_HHmm}.pdf";

            var pdfBytes = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(20);

                    page.Header().Column(col =>
                    {
                        col.Spacing(5);
                        col.Item().AlignCenter().Text("Class Report")
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
                        HeaderCell("Class");

                        foreach (var sc in subCategories)
                        {
                            table.Cell().Padding(3).Text(counter++.ToString());
                            table.Cell().Padding(3).Text(sc.Category?.Name ?? "");
                            table.Cell().Padding(3).Text(sc.Name ?? "");
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
