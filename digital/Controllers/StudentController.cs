using AutoMapper;
using digital.Interfaces;
using digital.Models;
using digital.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.IO;


namespace digital.Controllers
{
    public class StudentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IStudentRepository _studentRepository;
        private readonly IGenericRepository<Student> _studentRepo;
        private readonly IMapper _mapper;

        public StudentController(
            ApplicationDbContext context,
            IStudentRepository studentRepository,
            IGenericRepository<Student> studentRepo,
            IMapper mapper)
        {
            _context = context;
            _studentRepository = studentRepository;
            _studentRepo = studentRepo;
            _mapper = mapper;
        }

        [HttpGet]
        public IActionResult Student()
        {
            var userRole = HttpContext.Session.GetString("UserRole");

            if (userRole == "Student")
            {
                var studentId = HttpContext.Session.GetInt32("StudentId");
                var student = _studentRepository.GetStudentById(studentId.Value);

                if (student != null)
                {
                    var vm = _mapper.Map<StudentViewModel>(student);
                    vm.Categories = _context.Categories
                        .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name }).ToList();
                    vm.SubCategories = _context.SubCategories
                        .Select(sc => new SelectListItem { Value = sc.Id.ToString(), Text = sc.Name }).ToList();

                    return View(vm);
                }
            }

            var studentVM = new StudentViewModel
            {
                Categories = _context.Categories
                    .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name }).ToList(),
                SubCategories = new List<SelectListItem>(),
                StudentList = _studentRepository.GetAllStudentsWithCategoryAndSubCategory()
            };

            return View(studentVM);
        }

        [HttpPost]
        public IActionResult Student(StudentViewModel studentVM)
        {
            if (ModelState.IsValid)
            {
                var student = _mapper.Map<Student>(studentVM);
                student.CreatedDate = DateTime.Now;

                _context.Student.Add(student);
                _context.SaveChanges();

                var user = new User
                {
                    Name = student.Name,
                    Email = student.Email,
                    Password = student.Password,
                    Role = "Student",
                    StudentId = student.Id
                };

                _context.Users.Add(user);
                _context.SaveChanges();

                TempData["Success"] = "Student Registered Successfully!";
                return RedirectToAction("Student");
            }

            studentVM.Categories = _context.Categories
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name }).ToList();
            studentVM.SubCategories = _context.SubCategories
                .Where(sc => sc.CategoryId == studentVM.CategoryId)
                .Select(sc => new SelectListItem { Value = sc.Id.ToString(), Text = sc.Name }).ToList();
            studentVM.StudentList = _context.Student.ToList();

            return View(studentVM);
        }

        [HttpGet]
        public async Task<IActionResult> EditStudent(int id)
        {
            var student = await _studentRepo.GetByIdAsync(id);
            if (student == null) return NotFound();

            var vm = _mapper.Map<StudentViewModel>(student);

            ViewBag.Categories = _context.Categories
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
                .ToList();

            ViewBag.SubCategories = _context.SubCategories
                .Where(sc => sc.CategoryId == student.CategoryId)
                .Select(sc => new SelectListItem { Value = sc.Id.ToString(), Text = sc.Name })
                .ToList();

            return View("EditStudent", vm);
        }


        [HttpPost]
        public async Task<IActionResult> EditStudent(StudentViewModel studentVM)
        {
            if (ModelState.IsValid)
            {
                var student = _mapper.Map<Student>(studentVM);
                await _studentRepo.UpdateAsync(student);
                await _studentRepo.SaveAsync();

                TempData["Success"] = "Student Updated Successfully!";
                return RedirectToAction("Student");
            }

            ViewBag.Categories = _context.Categories
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
                .ToList();

            ViewBag.SubCategories = _context.SubCategories
                .Where(sc => sc.CategoryId == studentVM.CategoryId)
                .Select(sc => new SelectListItem { Value = sc.Id.ToString(), Text = sc.Name })
                .ToList();

            return View("EditStudent", studentVM);
        }


        public async Task<IActionResult> DeleteStudent(int id)
        {
            var student = await _studentRepo.GetByIdAsync(id);
            if (student != null)
            {
                await _studentRepo.DeleteAsync(student);
                await _studentRepo.SaveAsync();
            }
            return RedirectToAction("Student");
        }

        [HttpGet]
        public IActionResult StudentDetails()
        {
            var studentId = HttpContext.Session.GetInt32("StudentId");
            if (studentId == null)
                return RedirectToAction("StudentDetails", "Student");

            var student = _context.Student.FirstOrDefault(s => s.Id == studentId.Value);
            if (student == null)
                return NotFound();

            StudentViewModel vm = new StudentViewModel
            {
                Id = student.Id,
                Name = student.Name,
                Email = student.Email,
                Password = student.Password,
                CategoryId = student.CategoryId,
                SubCategoryId = student.SubCategoryId,
                DOB = student.DOB,
                Gender = student.Gender,
                MobileNumber = student.MobileNumber,
                Address = student.Address,
                CreatedDate = student.CreatedDate
            };

            ViewBag.Years = Enumerable.Range(2025, 26).ToList();
            ViewBag.Months = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.MonthNames
                                .Where(m => !string.IsNullOrEmpty(m))
                                .Select((name, index) => new { Name = name, Value = index + 1 })
                                .ToList();

            return View(vm);
        }

        [HttpGet]
        public IActionResult GetStudentAttendance(int studentId, int month, int year)
        {
            var totalDays = DateTime.DaysInMonth(year, month);

            var attendanceData = _context.Attendance
                .Where(a => a.StudentId == studentId && a.Month == month && a.Year == year)
                .ToList();

            var attendanceMap = attendanceData.ToDictionary(
                a => a.Day,
                a => a.Attend?.Trim().ToLower() == "yes"
            );

            int totalPresent = attendanceMap.Count(kvp => kvp.Value);
            int totalAbsent = attendanceMap.Count(kvp => !kvp.Value);

            ViewBag.TotalDays = totalDays;
            ViewBag.AttendanceMap = attendanceMap;
            ViewBag.TotalPresent = totalPresent;
            ViewBag.TotalAbsent = totalAbsent;
            ViewBag.SelectedMonth = month;
            ViewBag.SelectedYear = year;

            return PartialView("_StudentAttendanceTable");
        }
        [HttpGet]
        public JsonResult GetSubCategories(int categoryId)
        {
            var subcategories = _context.SubCategories
                .Where(sc => sc.CategoryId == categoryId)
                .Select(sc => new { id = sc.Id, name = sc.Name })
                .ToList();

            return Json(subcategories);
        }

        [HttpGet]
        public IActionResult ExportStudentsToExcel()
        {
            var students = _studentRepository.GetAllStudentsWithCategoryAndSubCategory();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Students");


            worksheet.Cell(1, 1).Value = "No.";
            worksheet.Cell(1, 2).Value = "Name";
            worksheet.Cell(1, 3).Value = "Email";
            worksheet.Cell(1, 4).Value = "Standard";
            worksheet.Cell(1, 5).Value = "Class";
            worksheet.Cell(1, 6).Value = "DOB";
            worksheet.Cell(1, 7).Value = "Gender";
            worksheet.Cell(1, 8).Value = "Mobile";
            worksheet.Cell(1, 9).Value = "Address";

            int row = 2, counter = 1;
            foreach (var s in students)
            {
                worksheet.Cell(row, 1).Value = counter++;
                worksheet.Cell(row, 2).Value = s.Name;
                worksheet.Cell(row, 3).Value = s.Email;
                worksheet.Cell(row, 4).Value = s.Category?.Name ?? "";
                worksheet.Cell(row, 5).Value = s.SubCategory?.Name ?? "";
                worksheet.Cell(row, 6).Value = s.DOB.ToString("yyyy-MM-dd");
                worksheet.Cell(row, 7).Value = s.Gender;
                worksheet.Cell(row, 8).Value = s.MobileNumber;
                worksheet.Cell(row, 9).Value = s.Address;
                row++;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var content = stream.ToArray();

            return File(content,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "Students.xlsx");
        }

        [HttpGet]
        public IActionResult ExportStudentsToPdf()
        {
            var students = _studentRepository.GetAllStudentsWithCategoryAndSubCategory();
            int counter = 1;

            var fileName = $"Students_{DateTime.Now:yyyyMMdd_HHmm}.pdf";

            var pdfBytes = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(20);

                    page.Header().Column(col =>
                    {
                        col.Spacing(5);
                        col.Item().AlignCenter().Text("Student Report")
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
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(4);
                        });

                        void HeaderCell(string text) =>
                            table.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text(text).SemiBold();

                        HeaderCell("No.");
                        HeaderCell("Name");
                        HeaderCell("Email");
                        HeaderCell("Standard");
                        HeaderCell("Class");
                        HeaderCell("DOB");
                        HeaderCell("Gender");
                        HeaderCell("Mobile");
                        HeaderCell("Address");

                        foreach (var s in students)
                        {
                            table.Cell().Padding(3).Text(counter++.ToString());
                            table.Cell().Padding(3).Text(s.Name ?? "");
                            table.Cell().Padding(3).Text(s.Email ?? "");
                            table.Cell().Padding(3).Text(s.Category?.Name ?? "");
                            table.Cell().Padding(3).Text(s.SubCategory?.Name ?? "");
                            table.Cell().Padding(3).Text(s.DOB.ToString("yyyy-MM-dd"));
                            table.Cell().Padding(3).Text(s.Gender ?? "");
                            table.Cell().Padding(3).Text(s.MobileNumber ?? "");
                            table.Cell().Padding(3).Text(s.Address ?? "");
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


        public async Task<IActionResult> ExportStudentExamResultExcel(int studentId)
        {
            
            var allSubjects = await _context.Subjects.ToListAsync();

            
            var results = await (from r in _context.StudentExamResults
                                 join s in _context.Subjects on r.SubjectId equals s.Id
                                 where r.StudentId == studentId
                                 select new
                                 {
                                     SubjectName = s.Name,
                                     r.TotalQuestions,
                                     r.CorrectAnswers
                                 }).ToListAsync();

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Exam Results");

         
            ws.Cell(1, 1).Value = "Subject";
            int col = 2;
            foreach (var subj in allSubjects)
            {
                ws.Cell(1, col).Value = subj.Name;
                col++;
            }

            
            ws.Cell(2, 1).Value = "Total Marks";
            col = 2;
            foreach (var subj in allSubjects)
            {
                var res = results.Where(r => r.SubjectName == subj.Name).ToList();
                if (res.Any())
                {
                    ws.Cell(2, col).Value = res.Sum(x => x.TotalQuestions);
                }
                else
                {
                    ws.Cell(2, col).Value = "Not Attended";
                }
                col++;
            }


            ws.Cell(3, 1).Value = "Correct Answers";
            col = 2;
            foreach (var subj in allSubjects)
            {
                var res = results.Where(r => r.SubjectName == subj.Name).ToList();
                if (res.Any())
                {
                    ws.Cell(3, col).Value = res.Sum(x => x.CorrectAnswers);
                }
                else
                {
                    ws.Cell(3, col).Value = "Not Attended";
                }
                col++;
            }

           
            ws.Cell(4, 1).Value = "Percentage";
            col = 2;
            foreach (var subj in allSubjects)
            {
                var res = results.Where(r => r.SubjectName == subj.Name).ToList();
                if (res.Any())
                {
                    int total = res.Sum(x => x.TotalQuestions);
                    int correct = res.Sum(x => x.CorrectAnswers);
                    double percentage = total == 0 ? 0 : (correct * 100.0 / total);
                    ws.Cell(4, col).Value = $"{percentage:F2}%";

                    
                    if (percentage < 35)
                    {
                        ws.Cell(4, col).Style.Fill.BackgroundColor = XLColor.Red;
                        ws.Cell(4, col).Style.Font.FontColor = XLColor.White;
                    }
                }
                else
                {
                    ws.Cell(4, col).Value = "Not Attended";
                }
                col++;
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            string fileName = $"Student_{studentId}_ExamResults.xlsx";
            return File(stream.ToArray(),
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        fileName);
        }

        public async Task<IActionResult> ExportStudentExamResultPdf(int studentId)
        {
            var allSubjects = await _context.Subjects.ToListAsync();

            var results = await (from r in _context.StudentExamResults
                                 join s in _context.Subjects on r.SubjectId equals s.Id
                                 where r.StudentId == studentId
                                 select new
                                 {
                                     SubjectName = s.Name,
                                     r.TotalQuestions,
                                     r.CorrectAnswers
                                 }).ToListAsync();

            
            var data = new Dictionary<string, List<string>>();

            data["Total Marks"] = new List<string>();
            data["Correct Answers"] = new List<string>();
            data["Percentage"] = new List<string>();

            foreach (var subj in allSubjects)
            {
                var res = results.Where(r => r.SubjectName == subj.Name).ToList();
                if (res.Any())
                {
                    int total = res.Sum(x => x.TotalQuestions);
                    int correct = res.Sum(x => x.CorrectAnswers);
                    double percentage = total == 0 ? 0 : (correct * 100.0 / total);

                    data["Total Marks"].Add(total.ToString());
                    data["Correct Answers"].Add(correct.ToString());
                    data["Percentage"].Add($"{percentage:F2}%");
                }
                else
                {
                    data["Total Marks"].Add("Not Attended");
                    data["Correct Answers"].Add("Not Attended");
                    data["Percentage"].Add("Not Attended");
                }
            }

            var pdf = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);
                    page.Header().Text($"Exam Results for Student {studentId}").Bold().FontSize(16);
                    page.Content().Table(table =>
                    {
                        
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(150);
                            foreach (var subj in allSubjects)
                                columns.RelativeColumn();
                        });

                        
                        table.Cell().Text("Subject").Bold();
                        foreach (var subj in allSubjects)
                            table.Cell().Text(subj.Name).Bold();

                        
                        foreach (var rowKey in data.Keys)
                        {
                            table.Cell().Text(rowKey).Bold();

                            for (int i = 0; i < allSubjects.Count; i++)
                            {
                                string value = data[rowKey][i];
                                if (rowKey == "Percentage" && double.TryParse(value.Replace("%", ""), out double perc) && perc < 35)
                                {
                                    table.Cell().Background(Colors.Red.Medium).Padding(5).Text(value).FontColor(Colors.White);
                                }
                                else
                                {
                                    table.Cell().Padding(5).Text(value);
                                }
                            }
                        }
                    });
                });
            });

            using var stream = new MemoryStream();
            pdf.GeneratePdf(stream);
            stream.Position = 0;

            string fileName = $"Student_{studentId}_ExamResults.pdf";
            return File(stream.ToArray(), "application/pdf", fileName);
        }


    }
}