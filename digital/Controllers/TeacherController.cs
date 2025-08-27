using ClosedXML.Excel;
using digital.Models;
using digital.Repository;
using digital.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.IO;

namespace digital.Controllers
{
    public class TeacherController : Controller
    {
        private readonly ITeacherRepository _repository;

        public TeacherController(ITeacherRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public IActionResult TeacherRegister()
        {
            ViewBag.TeacherList = _repository.GetAllTeachers().ToList();
            return View(new TeacherViewModel());
        }

        [HttpPost]
        public IActionResult TeacherRegister(TeacherViewModel model)
        {
            if (ModelState.IsValid)
            {
                
                var teacher = new Teacher
                {
                    Name = model.Name,
                    Email = model.Email,
                    Password = model.Password,
                    DOB = model.DOB,
                    Gender = model.Gender,
                    Address = model.Address
                };

                _repository.AddTeacher(teacher);
                _repository.Save(); 

                
                var user = new User
                {
                    Name = teacher.Name,
                    Email = teacher.Email,
                    Password = teacher.Password,
                    Role = "Teacher",
                    TeacherId = teacher.TeacherId,
                    CurrentSessionId = _repository.GetCurrentSessionId().ToString() 
                };

                _repository.AddUser(user);
                _repository.Save();

                return RedirectToAction("TeacherRegister", "Teacher");
            }

            ViewBag.TeacherList = _repository.GetAllTeachers().ToList();
            return View(model);
        }


        [HttpGet]
        public IActionResult Edit(int id)
        {
            var teacher = _repository.GetTeacherById(id);
            if (teacher == null)
            {
                return NotFound();
            }

            var model = new TeacherViewModel
            {
                Name = teacher.Name,
                Email = teacher.Email,
                Password = teacher.Password,
                DOB = teacher.DOB,
                Gender = teacher.Gender,
                Address = teacher.Address
            };

            ViewBag.TeacherId = id;
            return View(model);
        }

        [HttpPost]
        public IActionResult Edit(int id, TeacherViewModel model)
        {
            if (ModelState.IsValid)
            {
                var teacher = _repository.GetTeacherById(id);
                if (teacher == null)
                {
                    return NotFound();
                }

                teacher.Name = model.Name;
                teacher.Email = model.Email;
                teacher.Password = model.Password;
                teacher.DOB = model.DOB;
                teacher.Gender = model.Gender;
                teacher.Address = model.Address;

                _repository.UpdateTeacher(teacher);

                var user = _repository.GetUserByTeacherId(id);
                if (user != null)
                {
                    user.Name = model.Name;
                    user.Email = model.Email;
                    user.Password = model.Password;
                    _repository.UpdateUser(user);
                }

                _repository.Save();
                return RedirectToAction("TeacherRegister");
            }

            ViewBag.TeacherId = id;
            return View(model);
        }

        public IActionResult Delete(int id)
        {
            _repository.DeleteTeacher(id);
            _repository.DeleteUser(id);
            _repository.Save();

            return RedirectToAction("TeacherRegister");
        }

        [HttpGet]
        public IActionResult ExportTeachersToExcel()
        {
            var teachers = _repository.GetAllTeachers();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Teachers");


            worksheet.Cell(1, 1).Value = "No.";
            worksheet.Cell(1, 2).Value = "Name";
            worksheet.Cell(1, 3).Value = "Email";
            worksheet.Cell(1, 4).Value = "DOB";
            worksheet.Cell(1, 5).Value = "Gender";
            worksheet.Cell(1, 6).Value = "Address";

            int row = 2, counter = 1;
            foreach (var t in teachers)
            {
                worksheet.Cell(row, 1).Value = counter++;
                worksheet.Cell(row, 2).Value = t.Name;
                worksheet.Cell(row, 3).Value = t.Email;
                worksheet.Cell(row, 4).Value = t.DOB.ToString("yyyy-MM-dd");
                worksheet.Cell(row, 5).Value = t.Gender;
                worksheet.Cell(row, 6).Value = t.Address;
                row++;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var content = stream.ToArray();

            return File(content,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "Teachers.xlsx");
        }

        [HttpGet]
        public IActionResult ExportTeachersToPdf()
        {
            var teachers = _repository.GetAllTeachers();
            int counter = 1;
            var fileName = $"Teachers_{DateTime.Now:yyyyMMdd_HHmm}.pdf";

            var pdfBytes = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(20);

                    page.Header().Column(col =>
                    {
                        col.Spacing(5);
                        col.Item().AlignCenter().Text("Teacher Report")
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
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(4);
                        });

                        void HeaderCell(string text) =>
                            table.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text(text).SemiBold();

                        HeaderCell("No.");
                        HeaderCell("Name");
                        HeaderCell("Email");
                        HeaderCell("DOB");
                        HeaderCell("Gender");
                        HeaderCell("Address");

                        foreach (var t in teachers)
                        {
                            table.Cell().Padding(3).Text(counter++.ToString());
                            table.Cell().Padding(3).Text(t.Name ?? "");
                            table.Cell().Padding(3).Text(t.Email ?? "");
                            table.Cell().Padding(3).Text(t.DOB.ToString("yyyy-MM-dd"));
                            table.Cell().Padding(3).Text(t.Gender ?? "");
                            table.Cell().Padding(3).Text(t.Address ?? "");
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
