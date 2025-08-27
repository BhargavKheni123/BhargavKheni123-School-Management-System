using digital.Interfaces;
using digital.Models;
using digital.Repositories;
using Microsoft.AspNetCore.Mvc;
using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.IO;

namespace digital.Controllers
{
    public class TeacherMasterController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly ITeacherMasterRepository _teacherMasterRepository;

        public TeacherMasterController(
            IUserRepository userRepository,
            ITeacherMasterRepository teacherMasterRepository)
        {
            _userRepository = userRepository;
            _teacherMasterRepository = teacherMasterRepository;
        }

        public IActionResult TeacherMaster()
        {
            ViewBag.Categories = _teacherMasterRepository.GetCategories();
            ViewBag.Subjects = _teacherMasterRepository.GetSubjects();
            ViewBag.Teachers = _userRepository.GetTeachers();

            var data = _teacherMasterRepository.GetAllWithRelations();
            return View("TeacherMaster", data);
        }

        [HttpGet]
        public IActionResult GetSubCategories(int categoryId)
        {
            var subCategories = _teacherMasterRepository.GetSubCategoriesByCategory(categoryId)
                .Select(sc => new { sc.Id, sc.Name }).ToList();

            return Json(subCategories);
        }

        [HttpGet]
        public IActionResult GetSubCategoriesbyteacher(int categoryId)
        {
            var subCategories = _teacherMasterRepository.GetSubCategoriesByCategory(categoryId)
                .Select(sc => new { sc.Id, sc.Name }).ToList();

            return Json(subCategories);
        }

        [HttpPost]
        public IActionResult SaveTeacherAssignment(int CategoryId, int SubCategoryId, int SubjectId, int TeacherId)
        {
            if (!_teacherMasterRepository.Exists(CategoryId, SubCategoryId, SubjectId, TeacherId))
            {
                var newItem = new TeacherMaster
                {
                    CategoryId = CategoryId,
                    SubCategoryId = SubCategoryId,
                    SubjectId = SubjectId,
                    TeacherId = TeacherId,
                    CreatedDate = DateTime.Now
                };

                _teacherMasterRepository.Add(newItem);
            }

            return RedirectToAction("TeacherMaster");
        }

        public IActionResult EditTeacherMaster(int id)
        {
            var item = _teacherMasterRepository.GetById(id);
            if (item == null)
                return NotFound();

            ViewBag.Categories = _teacherMasterRepository.GetCategories();
            ViewBag.Subjects = _teacherMasterRepository.GetSubjects();
            ViewBag.Teachers = _teacherMasterRepository.GetTeachers();
            ViewBag.SubCategories = _teacherMasterRepository.GetSubCategoriesByCategory(item.CategoryId);

            return View("EditTeacherMaster", item);
        }

        [HttpPost]
        public IActionResult UpdateTeacherMaster(TeacherMaster model)
        {
            var existing = _teacherMasterRepository.GetById(model.Id);
            if (existing == null)
                return NotFound();

            existing.CategoryId = model.CategoryId;
            existing.SubCategoryId = model.SubCategoryId;
            existing.SubjectId = model.SubjectId;
            existing.TeacherId = model.TeacherId;

            _teacherMasterRepository.Update(existing);

            return RedirectToAction("TeacherMaster");
        }

        public IActionResult DeleteTeacherMaster(int id)
        {
            _teacherMasterRepository.Delete(id);
            return RedirectToAction("TeacherMaster");
        }

        public IActionResult ExportTeacherMasterToExcel()
        {
            var data = _teacherMasterRepository.GetAllWithRelations();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("TeacherAssignments");

                
                worksheet.Cell(1, 1).Value = "Standard";
                worksheet.Cell(1, 2).Value = "Class";
                worksheet.Cell(1, 3).Value = "Subject";
                worksheet.Cell(1, 4).Value = "Teacher";
                worksheet.Cell(1, 5).Value = "Assigned Date";

                int row = 2;
                foreach (var item in data)
                {
                    worksheet.Cell(row, 1).Value = item.Category?.Name ?? "";
                    worksheet.Cell(row, 2).Value = item.SubCategory?.Name ?? "";
                    worksheet.Cell(row, 3).Value = item.Subject?.Name ?? "";
                    worksheet.Cell(row, 4).Value = item.Teacher?.Name ?? "";
                    worksheet.Cell(row, 5).Value = item.CreatedDate.ToString("dd-MM-yyyy");
                    row++;
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return File(stream.ToArray(),
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "TeacherAssignments.xlsx");
                }
            }
        }

        public IActionResult ExportTeacherMasterToPdf()
        {
            var data = _teacherMasterRepository.GetAllWithRelations();

            var pdf = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);
                    page.Size(PageSizes.A4);

                    page.Header()
                        .Text("Teacher Assignment Report")
                        .FontSize(18)
                        .Bold()
                        .AlignCenter();

                    page.Content()
                        .Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(100); 
                                columns.ConstantColumn(100); 
                                columns.ConstantColumn(100);
                                columns.ConstantColumn(120); 
                                columns.ConstantColumn(100); 
                            });

                            
                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Standard").Bold();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Class").Bold();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Subject").Bold();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Teacher").Bold();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Assigned Date").Bold();
                            });

                            
                            foreach (var item in data)
                            {
                                table.Cell().Padding(5).Text(item.Category?.Name ?? "");
                                table.Cell().Padding(5).Text(item.SubCategory?.Name ?? "");
                                table.Cell().Padding(5).Text(item.Subject?.Name ?? "");
                                table.Cell().Padding(5).Text(item.Teacher?.Name ?? "");
                                table.Cell().Padding(5).Text(item.CreatedDate.ToString("dd-MM-yyyy"));
                            }
                        });
                });
            });

            var stream = new MemoryStream();
            pdf.GeneratePdf(stream);
            return File(stream.ToArray(), "application/pdf", "TeacherAssignments.pdf");
        }

    }
}
