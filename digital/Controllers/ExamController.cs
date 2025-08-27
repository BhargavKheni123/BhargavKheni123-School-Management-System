using ClosedXML.Excel;
using digital.Models;
using digital.Repository;
using digital.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.IO;

namespace digital.Controllers
{
    public class ExamController : Controller
    {
        private readonly IExamRepository _repository;

        public ExamController(IExamRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public IActionResult CreateExam()
        {
            var loggedInUserEmail = User.Identity?.Name ?? HttpContext.Session.GetString("UserEmail");
            var loggedInTeacher = _repository.GetTeacherByEmail(loggedInUserEmail);

            var model = new ExamViewModel
            {
                Categories = _repository.GetCategories()
                    .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name }).ToList(),
                Subjects = _repository.GetSubjects()
                    .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name }).ToList(),
                Teachers = _repository.GetTeachers()
                    .Select(t => new SelectListItem { Value = t.Id.ToString(), Text = t.Name }).ToList(),
                ExamList = (User.IsInRole("Teacher") && loggedInTeacher != null)
                    ? _repository.GetExamsByTeacherId(loggedInTeacher.Id).ToList()
                    : _repository.GetAllExams().ToList()
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult CreateExam(ExamViewModel model)
        {
            model.Categories = _repository.GetCategories()
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name }).ToList();
            model.Subjects = _repository.GetSubjects()
                .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name }).ToList();
            model.Teachers = _repository.GetTeachers()
                .Select(t => new SelectListItem { Value = t.Id.ToString(), Text = t.Name }).ToList();

            var exam = new Exam
            {
                ExamTitle = model.ExamTitle,
                Description = model.Description,
                ExamType = model.ExamType,
                CategoryId = model.CategoryId.GetValueOrDefault(),
                SubjectId = model.SubjectId.GetValueOrDefault(),
                AssignedTeacherId = model.AssignedTeacherId,
                ExamDate = model.ExamDate,
                CreatedBy = 1,
                CreatedDate = DateTime.Now
            };

            _repository.AddExam(exam);

            TempData["Success"] = "Exam created successfully!";
            return RedirectToAction("CreateExam");
        }

        [HttpGet]
        public IActionResult EditExam(int id)
        {
            var exam = _repository.GetExamById(id);
            if (exam == null)
                return RedirectToAction("CreateExam");

            var model = new ExamViewModel
            {
                ExamId = exam.ExamId,
                ExamTitle = exam.ExamTitle,
                Description = exam.Description,
                ExamType = exam.ExamType,
                CategoryId = exam.CategoryId,
                SubjectId = exam.SubjectId,
                AssignedTeacherId = exam.AssignedTeacherId,
                ExamDate = exam.ExamDate,

                Categories = _repository.GetCategories()
                    .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name }).ToList(),
                Subjects = _repository.GetSubjects()
                    .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name }).ToList(),
                Teachers = _repository.GetTeachers()
                    .Select(t => new SelectListItem { Value = t.Id.ToString(), Text = t.Name }).ToList()
            };

            return View("EditExam", model);
        }

        [HttpPost]
        public IActionResult EditExam(ExamViewModel model)
        {
            var exam = _repository.GetExamById(model.ExamId);
            if (exam == null)
                return RedirectToAction("CreateExam");

            exam.ExamTitle = model.ExamTitle;
            exam.Description = model.Description;
            exam.ExamType = model.ExamType;
            exam.CategoryId = model.CategoryId.GetValueOrDefault();
            exam.SubjectId = model.SubjectId.GetValueOrDefault();
            exam.AssignedTeacherId = model.AssignedTeacherId;
            exam.ExamDate = model.ExamDate;

            _repository.UpdateExam(exam);

            TempData["Success"] = "Exam updated successfully!";
            return RedirectToAction("CreateExam");
        }

        [HttpGet]
        public IActionResult DeleteExam(int id)
        {
            _repository.DeleteExam(id);
            TempData["Success"] = "Exam deleted successfully!";
            return RedirectToAction("CreateExam");
        }

        [HttpGet]
        public IActionResult StudentExamList()
        {
            var studentId = HttpContext.Session.GetInt32("StudentId");
            if (studentId == null)
                return RedirectToAction("StudentDetails", "Student");

            var student = _repository.GetStudentById(studentId.Value);
            if (student == null)
                return NotFound();

            var exams = _repository.GetExamsByCategoryId(student.CategoryId).ToList();

            var model = new ExamViewModel { ExamList = exams };
            return View(model);
        }

        [HttpGet]
        public IActionResult TeacherExamList()
        {
            var loggedInUserEmail = User.Identity?.Name ?? HttpContext.Session.GetString("UserEmail");
            var teacher = _repository.GetTeacherByEmail(loggedInUserEmail);

            if (teacher == null)
            {
                ViewBag.Message = "No exams found for this teacher.";
                return View(new ExamViewModel { ExamList = new List<ExamListItem>() });
            }

            var exams = _repository.GetExamsByTeacherId(teacher.Id).ToList();

            var model = new ExamViewModel { ExamList = exams };
            return View(model);
        }

        [HttpGet]
        public IActionResult ExportExamsToExcel()
        {
            var exams = _repository.GetAllExams();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Exams");

            
            worksheet.Cell(1, 1).Value = "No.";
            worksheet.Cell(1, 2).Value = "Title";
            worksheet.Cell(1, 3).Value = "Type";
            worksheet.Cell(1, 4).Value = "Class";
            worksheet.Cell(1, 5).Value = "ExamDate";
            worksheet.Cell(1, 6).Value = "Subject";
            worksheet.Cell(1, 7).Value = "Teacher";

            int row = 2, counter = 1;
            foreach (var e in exams)
            {
                worksheet.Cell(row, 1).Value = counter++;
                worksheet.Cell(row, 2).Value = e.ExamTitle;
                worksheet.Cell(row, 3).Value = e.ExamType;
                worksheet.Cell(row, 4).Value = e.ClassName ?? "";
                worksheet.Cell(row, 5).Value = e.ExamDate?.ToString("yyyy-MM-dd") ?? "";
                worksheet.Cell(row, 6).Value = e.SubjectName ?? "";
                worksheet.Cell(row, 7).Value = e.TeacherName ?? "";
                row++;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var content = stream.ToArray();

            return File(content,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "Exams.xlsx");
        }

        [HttpGet]
        public IActionResult ExportExamsToPdf()
        {
            var exams = _repository.GetAllExams();
            int counter = 1;
            var fileName = $"Exams_{DateTime.Now:yyyyMMdd_HHmm}.pdf";

            var pdfBytes = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(20);

                    page.Header().Column(col =>
                    {
                        col.Spacing(5);
                        col.Item().AlignCenter().Text("Exam Report").FontSize(18).SemiBold();
                        col.Item().LineHorizontal(1);
                    });

                    page.Content().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(1); 
                            columns.RelativeColumn(3); 
                            columns.RelativeColumn(2); 
                            columns.RelativeColumn(2); 
                            columns.RelativeColumn(2); 
                            columns.RelativeColumn(3); 
                            columns.RelativeColumn(3); 
                        });

                        void HeaderCell(string text) =>
                            table.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text(text).SemiBold();

                        HeaderCell("No.");
                        HeaderCell("Title");
                        HeaderCell("Type");
                        HeaderCell("Class");
                        HeaderCell("ExamDate");
                        HeaderCell("Subject");
                        HeaderCell("Teacher");

                        foreach (var e in exams)
                        {
                            table.Cell().Padding(3).Text(counter++.ToString());
                            table.Cell().Padding(3).Text(e.ExamTitle ?? "");
                            table.Cell().Padding(3).Text(e.ExamType ?? "");
                            table.Cell().Padding(3).Text(e.ClassName ?? "");
                            table.Cell().Padding(3).Text(e.ExamDate?.ToString("yyyy-MM-dd") ?? "");
                            table.Cell().Padding(3).Text(e.SubjectName ?? "");
                            table.Cell().Padding(3).Text(e.TeacherName ?? "");
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
