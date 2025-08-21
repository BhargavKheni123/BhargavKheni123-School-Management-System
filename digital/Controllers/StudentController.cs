using AutoMapper;
using digital.Interfaces;
using digital.Models;
using digital.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

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
            vm.Categories = _context.Categories
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name }).ToList();
            vm.SubCategories = _context.SubCategories
                .Where(sc => sc.CategoryId == student.CategoryId)
                .Select(sc => new SelectListItem { Value = sc.Id.ToString(), Text = sc.Name }).ToList();

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

            studentVM.Categories = _context.Categories
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name }).ToList();
            studentVM.SubCategories = _context.SubCategories
                .Where(sc => sc.CategoryId == studentVM.CategoryId)
                .Select(sc => new SelectListItem { Value = sc.Id.ToString(), Text = sc.Name }).ToList();

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

    }
}