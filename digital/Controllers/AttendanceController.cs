using digital.Interfaces;
using digital.Models;
using digital.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace digital.Controllers
{
    public class AttendanceController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IAttendanceRepository _attendanceRepository;

        public AttendanceController(ApplicationDbContext context, IAttendanceRepository attendanceRepository)
        {
            _context = context;
            _attendanceRepository = attendanceRepository;
        }

        [HttpGet]
        public IActionResult AttendanceForm(int? SelectedCategoryId, int? SelectedSubCategoryId, int? Month, int? Year)
        {
            string role = HttpContext.Session.GetString("UserRole");
            string email = HttpContext.Session.GetString("UserEmail");

            var model = new AttendanceViewModel
            {
                SelectedCategoryId = SelectedCategoryId,
                SelectedSubCategoryId = SelectedSubCategoryId,
                SelectedMonth = Month,
                SelectedYear = Year,
                Categories = _context.Categories.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                }).ToList(),
                SubCategories = SelectedCategoryId.HasValue
                    ? _context.SubCategories.Where(sc => sc.CategoryId == SelectedCategoryId.Value)
                        .Select(sc => new SelectListItem
                        {
                            Value = sc.Id.ToString(),
                            Text = sc.Name
                        }).ToList()
                    : new List<SelectListItem>(),
                IsStudent = role == "Student"
            };

            
            ViewBag.IsStudent = model.IsStudent;
            ViewBag.SelectedMonth = model.SelectedMonth ?? 0;
            ViewBag.SelectedYear = model.SelectedYear ?? 0;

            if (role == "Student")
            {
                var student = _context.Student.FirstOrDefault(s => s.Email == email);
                if (student == null)
                    return RedirectToAction("Login", "Account");

                model.Student = new List<Student> { student };
                model.TotalDays = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month);
                model.AttendanceData = _attendanceRepository.GetAttendanceByStudentId(student.Id);
                return View(model);
            }

            if (SelectedCategoryId.HasValue && SelectedSubCategoryId.HasValue && Month.HasValue && Year.HasValue)
            {
                var students = _context.Student
                    .Where(s => s.CategoryId == SelectedCategoryId.Value && s.SubCategoryId == SelectedSubCategoryId.Value)
                    .ToList();

                model.Student = students;
                model.TotalDays = DateTime.DaysInMonth(Year.Value, Month.Value);
                model.AttendanceData = _attendanceRepository.GetAttendanceByFilters(
                    students.Select(s => s.Id).ToList(), Month.Value, Year.Value);
            }

            return View(model);
        }

        [HttpPost]
        public IActionResult AttendanceForm(AttendanceViewModel model, IFormCollection form)
        {
            int catId = model.SelectedCategoryId ?? 0;
            int subCatId = model.SelectedSubCategoryId ?? 0;
            int month = model.SelectedMonth ?? 0;
            int year = model.SelectedYear ?? 0;

            var students = _context.Student
                .Where(s => s.CategoryId == catId && s.SubCategoryId == subCatId)
                .ToList();

            int totalDays = DateTime.DaysInMonth(year, month);
            var updatedRecords = new List<Attendance>();

            foreach (var student in students)
            {
                for (int d = 1; d <= totalDays; d++)
                {
                    string key = $"attend_{student.Id}_{d}";
                    string status = form[key];

                    updatedRecords.Add(new Attendance
                    {
                        StudentId = student.Id,
                        Day = d,
                        Month = month,
                        Year = year,
                        FullDate = new DateTime(year, month, d),
                        Attend = status
                    });
                }
            }

            _attendanceRepository.SaveAttendance(updatedRecords);

            model.Categories = _context.Categories.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name
            }).ToList();

            model.SubCategories = _context.SubCategories
                .Where(SC => SC.CategoryId == model.SelectedCategoryId)
                .Select(sc => new SelectListItem
                {
                    Value = sc.Id.ToString(),
                    Text = sc.Name
                }).ToList();

            model.Student = students;
            model.TotalDays = totalDays;
            model.AttendanceData = _attendanceRepository.GetAttendanceByFilters(students.Select(s => s.Id).ToList(), month, year);
            model.IsStudent = false;

            
            ViewBag.IsStudent = model.IsStudent;
            ViewBag.SelectedMonth = model.SelectedMonth ?? 0;
            ViewBag.SelectedYear = model.SelectedYear ?? 0;

            return View(model);
        }

        [HttpPost]
        public IActionResult SaveAttendanceAjax(int studentId, int day, int month, int year, string status)
        {
            var record = _context.Attendance.FirstOrDefault(a =>
                a.StudentId == studentId &&
                a.Day == day &&
                a.Month == month &&
                a.Year == year
            );

            if (record != null)
            {
                record.Attend = status;
                _context.Attendance.Update(record);
            }
            else
            {
                var newRecord = new Attendance
                {
                    StudentId = studentId,
                    Day = day,
                    Month = month,
                    Year = year,
                    FullDate = new DateTime(year, month, day),
                    Attend = status
                };
                _context.Attendance.Add(newRecord);
            }

            _context.SaveChanges();
            return Json(new { success = true });
        }

        [HttpGet]
        public IActionResult GetSubCategories(int categoryId)
        {
            var subCategories = _context.SubCategories
                .Where(sc => sc.CategoryId == categoryId)
                .Select(sc => new { id = sc.Id, name = sc.Name })
                .ToList();
            return Json(subCategories);
        }
    }
}
