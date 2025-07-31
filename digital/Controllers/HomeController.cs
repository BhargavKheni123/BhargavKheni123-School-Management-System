using digital.Interfaces;
using digital.Models;
using digital.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Principal;
using System.Xml;

namespace digital.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly string role;
        private readonly IUserRepository _userRepository;
        private readonly ITeacherMasterRepository _teacherMasterRepository;
        private readonly IAdminRepository _adminRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly IAttendanceRepository _attendanceRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ISubCategoryRepository _subCategoryRepository;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, IUserRepository userRepository, ITeacherMasterRepository teacherMasterRepository, IAdminRepository adminRepository, IStudentRepository studentRepository, IAttendanceRepository attendanceRepository, ICategoryRepository categoryRepository, ISubCategoryRepository subCategoryRepository)
        {
            _logger = logger;
            _context = context;
            _userRepository = userRepository;
            _teacherMasterRepository = teacherMasterRepository;
            _adminRepository = adminRepository;
            _studentRepository = studentRepository;
            _attendanceRepository = attendanceRepository;
            _categoryRepository = categoryRepository;
            _subCategoryRepository = subCategoryRepository;
        }

        public IActionResult Index()
        {
            var email = HttpContext.Session.GetString("UserEmail");

            if (string.IsNullOrEmpty(email))
            {
                ViewBag.UserName = "? Email not in session";
                return View();
            }

            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user == null)
            {
                ViewBag.UserName = $"? User not found for email: {email}";
                return View();
            }

            ViewBag.UserName = $"? Welcome, {user.Name}";
            return View();
        }



        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            var user = _userRepository.GetUserByEmailAndPassword(email, password);

            if (user == null)
            {
                ViewBag.Error = "Invalid email or password.";
                return View();
            }

            HttpContext.Session.SetString("UserName", user.Name);
            HttpContext.Session.SetString("UserEmail", user.Email);
            HttpContext.Session.SetString("UserRole", user.Role);

            if (user.Role == "Student" && user.StudentId.HasValue)
                HttpContext.Session.SetInt32("StudentId", user.StudentId.Value);

            if (user.Role == "Student") return RedirectToAction("Student", "Home");
            if (user.Role == "Admin") return RedirectToAction("Index", "Home");
            if (user.Role == "Teacher") return RedirectToAction("TimeTableForm", "Home");

            return RedirectToAction("Login");
        }





        private string GenerateNameFromEmail(string email)
        {
            var usernamePart = email.Split('@')[0]; 
            var parts = System.Text.RegularExpressions.Regex
                            .Split(usernamePart, @"(?<!^)(?=[A-Z])|[_\.\-\s]+");

            if (parts.Length == 1)
                return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(parts[0]);

            return string.Join(" ", parts.Select(p =>
                System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(p.ToLower())));
        }


        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }



        [HttpPost]
        public IActionResult Register(string name, string email, string password, string role)
        {
            var existingUser = _userRepository.GetUserByEmail(email);
            if (existingUser != null)
            {
                ViewBag.Error = "User already exists with this email.";
                return View();
            }

            var newUser = new User
            {
                Name = name,
                Email = email,
                Password = password,
                Role = role
            };

            _userRepository.AddUser(newUser);

            if (role == "Student")
            {
                var student = new Student
                {
                    Name = name,
                    Email = email,
                    Password = password,
                    CreatedDate = DateTime.Now,
                    CategoryId = 1,
                    SubCategoryId = 1,
                    DOB = DateTime.Now,
                    Gender = "Not Set",
                    MobileNumber = "0000000000",
                    Address = "Not Provided"
                };

                _studentRepository.AddStudent(student);

                HttpContext.Session.SetString("Email", student.Email);
                HttpContext.Session.SetString("Role", "Student");

                return RedirectToAction("Home", "Student");
            }

            return RedirectToAction("Login");
        }


        [HttpGet]
        public IActionResult Category()
        {
            var list = _categoryRepository.GetAllCategories();
            return View(list);
        }

        [HttpPost]
        public IActionResult Category(Category cat)
        {
            _categoryRepository.AddCategory(cat);
            return RedirectToAction("Category");
        }


        [HttpGet]
        public IActionResult Edit(int id)
        {
            var cat = _context.Categories.FirstOrDefault(c => c.Id == id);
            return View(cat);
        }

        [HttpPost]
        public IActionResult Edit(Category cat)
        {
            var existing = _context.Categories.FirstOrDefault(c => c.Id == cat.Id);
            if (existing != null)
            {
                existing.Name = cat.Name;
                _context.SaveChanges();
            }
            return RedirectToAction("Category");
        }

        public IActionResult Delete(int id)
        {
            var cat = _context.Categories.FirstOrDefault(c => c.Id == id);
            if (cat != null)
            {
                _context.Categories.Remove(cat);
                _context.SaveChanges();
            }
            return RedirectToAction("Category");
        }

        [HttpGet]
        public IActionResult Subcategories()
        {
            var vm = new SubCategoryViewModel
            {
                Categories = _categoryRepository.GetCategorySelectList(),
                SubCategoryList = _subCategoryRepository.GetSubCategoriesWithCategory()
            };
            return View(vm);
        }

        [HttpPost]
        public IActionResult Subcategories(int CategoryId, string Name)
        {
            if (!string.IsNullOrEmpty(Name) && CategoryId > 0)
            {
                var newSubCategory = new SubCategory
                {
                    CategoryId = CategoryId,
                    Name = Name
                };

                _subCategoryRepository.AddSubCategory(newSubCategory);
            }

            var vm = new SubCategoryViewModel
            {
                Categories = _categoryRepository.GetCategorySelectList(),
                SubCategoryList = _subCategoryRepository.GetSubCategoriesWithCategory()
            };

            return View(vm);
        }



        [HttpGet]
        public IActionResult EditSub(int id)
        {
            var subCategory = _context.SubCategories.FirstOrDefault(s => s.Id == id);
            if (subCategory == null)
            {
                return NotFound();
            }

            ViewBag.Categories = _context.Categories
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
                .ToList();

            return View(subCategory);
        }

        [HttpPost]
        public IActionResult EditSub(SubCategory updatedSubCategory)
        {
            var sub = _context.SubCategories.FirstOrDefault(s => s.Id == updatedSubCategory.Id);
            if (sub != null)
            {
                sub.CategoryId = updatedSubCategory.CategoryId;
                sub.Name = updatedSubCategory.Name;
                sub.CreatedDate = DateTime.Now;
                sub.CreatedBy = "admin";

                _context.SaveChanges();
                return RedirectToAction("Subcategories");
            }

            return NotFound();
        }

        [HttpPost]
        public IActionResult UpdateSub(int id, int CategoryId, string Name)
        {
            var sub = _context.SubCategories.Find(id);
            if (sub != null)
            {
                sub.CategoryId = CategoryId;
                sub.Name = Name;
                sub.CreatedDate = DateTime.Now;
                sub.CreatedBy = "admin"; 

                _context.SubCategories.Update(sub);
                _context.SaveChanges();
            }

            return RedirectToAction("Subcategories");
        }

        [HttpGet]
        public IActionResult DeleteSub(int id)
        {
            var sub = _context.SubCategories.Find(id);
            if (sub != null)
            {
                _context.SubCategories.Remove(sub);
                _context.SaveChanges();
            }

            return RedirectToAction("Subcategories");
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
                    ViewBag.Categories = _context.Categories.ToList();
                    ViewBag.SubCategories = _context.SubCategories.ToList();
                }

                return View(student);
            }

            ViewBag.Categories = new SelectList(_context.Categories.ToList(), "Id", "Name");
            ViewBag.SubCategories = new SelectList(new List<SelectListItem>());

            ViewBag.StudentList = _studentRepository.GetAllStudentsWithCategoryAndSubCategory();

            return View(new Student());
        }

        [HttpPost]
        public IActionResult Student(Student student)
        {
            if (ModelState.IsValid)
            {
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

            // Refill dropdowns etc.
            ViewBag.Categories = new SelectList(_context.Categories.ToList(), "Id", "Name", student.CategoryId);
            ViewBag.SubCategories = new SelectList(_context.SubCategories.Where(x => x.CategoryId == student.CategoryId).ToList(), "Id", "Name", student.SubCategoryId);
            ViewBag.StudentList = _context.Student.ToList();

            return View(student);
        }



        [HttpGet]
        public IActionResult EditStudent(int id)
        {
            var student = _context.Student.FirstOrDefault(x => x.Id == id);
            if (student == null) return NotFound();

            ViewBag.Categories = new SelectList(_context.Categories.ToList(), "Id", "Name", student.CategoryId);
            ViewBag.SubCategories = new SelectList(_context.SubCategories.Where(x => x.CategoryId == student.CategoryId).ToList(), "Id", "Name", student.SubCategoryId);

            return View("EditStudent", student);
        }
        [HttpPost]
        public IActionResult EditStudent(Student student)
        {
            if (ModelState.IsValid)
            {
                var existingStudent = _context.Student.FirstOrDefault(s => s.Id == student.Id);
                if (existingStudent == null)
                {
                    return NotFound();
                }

                existingStudent.Name = student.Name;
                existingStudent.CategoryId = student.CategoryId;
                existingStudent.SubCategoryId = student.SubCategoryId;
                existingStudent.DOB = student.DOB;
                existingStudent.Gender = student.Gender;
                existingStudent.MobileNumber = student.MobileNumber;
                existingStudent.Address = student.Address;
                existingStudent.Email = student.Email;
                existingStudent.Password = student.Password;

                _context.Student.Update(existingStudent);
                _context.SaveChanges();

                TempData["Success"] = "Student Updated Successfully!";
                return RedirectToAction("Student");
            }

            ViewBag.Categories = new SelectList(_context.Categories.ToList(), "Id", "Name", student.CategoryId);
            ViewBag.SubCategories = new SelectList(_context.SubCategories.Where(x => x.CategoryId == student.CategoryId).ToList(), "Id", "Name", student.SubCategoryId);
            return View("EditStudent", student);
        }


        public IActionResult DeleteStudent(int id)
        {
            var student = _context.Student.FirstOrDefault(x => x.Id == id);
            if (student != null)
            {
                _context.Student.Remove(student);
                _context.SaveChanges();
            }
            return RedirectToAction("Student");
        }
        [HttpGet]
        public JsonResult GetSubCategories(int categoryId)
        {
            var subCats = _context.SubCategories
                .Where(x => x.CategoryId == categoryId)
                .Select(x => new { x.Id, x.Name })
                .ToList();
            return Json(subCats);
        }


        [HttpGet]
        public IActionResult StudentDetails(int id)
        {
            var student = _context.Student.FirstOrDefault(s => s.Id == id);
            if (student == null)
                return NotFound();

            ViewBag.StudentId = student.Id;


            ViewBag.Years = Enumerable.Range(2025, 26).ToList();
            ViewBag.Months = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.MonthNames
                                .Where(m => !string.IsNullOrEmpty(m))
                                .Select((name, index) => new { Name = name, Value = index + 1 })
                                .ToList();

            return View(student);
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
        public IActionResult TimeTableForm()
        {
            var role = HttpContext.Session.GetString("UserRole");
            var email = HttpContext.Session.GetString("UserEmail");

            ViewBag.Role = role;

            var allTimetables = _context.TimeTables.ToList();

            ViewBag.TimeTableList = allTimetables;

            if (role == "Student")
            {
                return View("TimeTableForm");
            }

            ViewBag.StdList = _context.Categories.Select(c => new SelectListItem { Value = c.Name, Text = c.Name }).ToList();
            ViewBag.ClassList = _context.SubCategories.Select(sc => new SelectListItem { Value = sc.Name, Text = sc.Name }).ToList();
            ViewBag.Hours = Enumerable.Range(1, 24).Select(i => new SelectListItem { Value = i.ToString(), Text = i.ToString() }).ToList();
            ViewBag.Minutes = Enumerable.Range(1, 60).Select(i => new SelectListItem { Value = i.ToString(), Text = i.ToString() }).ToList();

            if (TempData["Message"] != null)
                ViewBag.Message = TempData["Message"];

            return View();
        }




        [HttpPost]
        public IActionResult TimeTableForm(TimeTable timeTable)
        {
            if (ModelState.IsValid)
            {
                _context.TimeTables.Add(timeTable);
                _context.SaveChanges();
                TempData["Message"] = "Time Table Saved!";
                return RedirectToAction("TimeTableForm");
            }


            ViewBag.StdList = _context.Categories.Select(c => new SelectListItem { Value = c.Name, Text = c.Name }).ToList();
            ViewBag.ClassList = _context.SubCategories.Select(sc => new SelectListItem { Value = sc.Name, Text = sc.Name }).ToList();
            ViewBag.Hours = Enumerable.Range(1, 24).Select(i => new SelectListItem { Value = i.ToString(), Text = i.ToString() }).ToList();
            ViewBag.Minutes = Enumerable.Range(1, 60).Select(i => new SelectListItem { Value = i.ToString(), Text = i.ToString() }).ToList();

            return View();
        }


        [HttpGet]
        public JsonResult GetSubCategoriesByStd(string stdName)
        {
            var subcategories = _context.SubCategories
                .Where(sc => sc.Category.Name == stdName)
                .Select(sc => new
                {
                    name = sc.Name
                }).ToList();

            return Json(subcategories);
        }





        [HttpGet]
        public IActionResult EditTimeTable(int id)
        {
            var record = _context.TimeTables.FirstOrDefault(x => x.Id == id);

            if (record == null)
                return NotFound();


            ViewBag.StdList = _context.Categories
                .Select(c => new SelectListItem { Value = c.Name, Text = c.Name }).ToList();

            ViewBag.ClassList = _context.SubCategories
                .Select(sc => new SelectListItem { Value = sc.Name, Text = sc.Name }).ToList();

            ViewBag.Hours = Enumerable.Range(1, 24)
                .Select(i => new SelectListItem { Value = i.ToString(), Text = i.ToString() }).ToList();

            ViewBag.Minutes = Enumerable.Range(1, 60)
                .Select(i => new SelectListItem { Value = i.ToString(), Text = i.ToString() }).ToList();

            return View("UpdateTimeTable", record);
        }

        [HttpPost]
        public IActionResult EditTimeTable(TimeTable model)
        {
            if (ModelState.IsValid)
            {
                _context.TimeTables.Update(model);
                _context.SaveChanges();
                TempData["Message"] = "Record updated successfully!";
                return RedirectToAction("TimeTableForm");
            }


            ViewBag.StdList = _context.Categories
                .Select(c => new SelectListItem { Value = c.Name, Text = c.Name }).ToList();

            ViewBag.ClassList = _context.SubCategories
                .Select(sc => new SelectListItem { Value = sc.Name, Text = sc.Name }).ToList();

            ViewBag.Hours = Enumerable.Range(1, 24)
                .Select(i => new SelectListItem { Value = i.ToString(), Text = i.ToString() }).ToList();

            ViewBag.Minutes = Enumerable.Range(1, 60)
                .Select(i => new SelectListItem { Value = i.ToString(), Text = i.ToString() }).ToList();

            return View("UpdateTimeTable", model);
        }

        public IActionResult DeleteTimeTable(int id)
        {
            var record = _context.TimeTables.FirstOrDefault(x => x.Id == id);
            if (record != null)
            {
                _context.TimeTables.Remove(record);
                _context.SaveChanges();
                TempData["Message"] = "Record deleted successfully!";
            }
            return RedirectToAction("TimeTableForm");
        }



        public IActionResult AttendanceForm(int? CategoryId, int? SubCategoryId, int? Month, int? Year)
        {
            string role = HttpContext.Session.GetString("UserRole");

            if (role == "Student")
            {
                string email = HttpContext.Session.GetString("UserEmail");
                var student = _context.Student.FirstOrDefault(s => s.Email == email);

                if (student == null)
                    return RedirectToAction("Login");

                var records = _attendanceRepository.GetAttendanceByStudentId(student.Id);
                ViewBag.IsStudent = true;
                return View(records);
            }

            ViewBag.IsStudent = false;
            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name", CategoryId);
            ViewBag.SubCategories = new SelectList(
                CategoryId.HasValue ? _context.SubCategories.Where(s => s.CategoryId == CategoryId) : new List<SubCategory>(),
                "Id", "Name", SubCategoryId);

            if (CategoryId.HasValue && SubCategoryId.HasValue && Month.HasValue && Year.HasValue)
            {
                var students = _context.Student
                    .Where(s => s.CategoryId == CategoryId && s.SubCategoryId == SubCategoryId)
                    .ToList();

                var ids = students.Select(s => s.Id).ToList();
                var attendance = _attendanceRepository.GetAttendanceByFilters(ids, Month.Value, Year.Value);

                ViewBag.Students = students;
                ViewBag.TotalDays = DateTime.DaysInMonth(Year.Value, Month.Value);
                ViewBag.SelectedMonth = Month;
                ViewBag.SelectedYear = Year;
                ViewBag.AttendanceData = attendance;
            }

            return View();
        }


        [HttpPost]
        public IActionResult AttendanceForm(IFormCollection form)
        {
            int catId = int.Parse(form["CategoryId"]);
            int subCatId = int.Parse(form["SubCategoryId"]);
            int month = int.Parse(form["Month"]);
            int year = int.Parse(form["Year"]);

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

            ViewBag.Categories = new SelectList(_context.Categories.ToList(), "Id", "Name", catId);
            ViewBag.SubCategories = new SelectList(_context.SubCategories.Where(x => x.CategoryId == catId), "Id", "Name", subCatId);
            ViewBag.Students = students;
            ViewBag.TotalDays = totalDays;
            ViewBag.SelectedMonth = month;
            ViewBag.SelectedYear = year;
            ViewBag.AttendanceData = _attendanceRepository.GetAttendanceByFilters(students.Select(s => s.Id).ToList(), month, year);

            return View();
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

        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Home");
        }

        public IActionResult TeacherMasterPage()
        {
            ViewBag.Categories = _context.Categories.ToList();
            ViewBag.Subjects = _context.Subject.ToList();
            ViewBag.Teachers = _userRepository.GetTeachers();

            var data = _teacherMasterRepository.GetAllWithRelations();

            return View("TeacherMaster", data); 
        }

        [HttpGet]
        public IActionResult GetSubCategoriesbyteacher(int categoryId)
        {
            var subCategories = _context.SubCategories
                .Where(sc => sc.CategoryId == categoryId)
                .Select(sc => new { sc.Id, sc.Name })
                .ToList();

            return Json(subCategories);
        }

        [HttpPost]
        public IActionResult SaveTeacherAssignment(int CategoryId, int SubCategoryId, int SubjectId, int TeacherId)
        {
            var exists = _context.TeacherMaster.FirstOrDefault(x =>
                x.CategoryId == CategoryId &&
                x.SubCategoryId == SubCategoryId &&
                x.SubjectId == SubjectId &&
                x.TeacherId == TeacherId);

            if (exists == null)
            {
                var newItem = new TeacherMaster
                {
                    CategoryId = CategoryId,
                    SubCategoryId = SubCategoryId,
                    SubjectId = SubjectId,
                    TeacherId = TeacherId,
                    CreatedDate = DateTime.Now
                };

                _context.TeacherMaster.Add(newItem);
                _context.SaveChanges();
            }

            return RedirectToAction("TeacherMasterPage");
        }



        public IActionResult EditTeacherMaster(int id)
        {
            var item = _context.TeacherMaster
                .Include(x => x.Category)
                .Include(x => x.SubCategory)
                .Include(x => x.Subject)
                .Include(x => x.Teacher)
                .FirstOrDefault(x => x.Id == id);

            if (item == null)
                return NotFound();

            ViewBag.Categories = _context.Categories.ToList();
            ViewBag.Subjects = _context.Subject.ToList();
            ViewBag.Teachers = _context.Users.Where(u => u.Role == "Teacher").ToList();
            ViewBag.SubCategories = _context.SubCategories
                .Where(s => s.CategoryId == item.CategoryId)
                .ToList();

            return View("EditTeacherMaster", item);
        }


        [HttpPost]
        public IActionResult UpdateTeacherMaster(TeacherMaster model)
        {
            var existing = _context.TeacherMaster.FirstOrDefault(x => x.Id == model.Id);
            if (existing == null)
                return NotFound();

            // Update only relevant fields
            existing.CategoryId = model.CategoryId;
            existing.SubCategoryId = model.SubCategoryId;
            existing.SubjectId = model.SubjectId;
            existing.TeacherId = model.TeacherId;
            // Do not update CreatedDate on edit!

            _context.SaveChanges();

            return RedirectToAction("TeacherMasterPage");
        }

        public IActionResult DeleteTeacherMaster(int id)
        {
            var item = _context.TeacherMaster.Find(id);
            if (item != null)
            {
                _context.TeacherMaster.Remove(item);
                _context.SaveChanges();
            }

            return RedirectToAction("TeacherMasterPage");
        }



        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}