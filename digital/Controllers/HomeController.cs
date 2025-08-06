using AutoMapper;
using digital.Interfaces;
using digital.Models;
using digital.Repositories;
using digital.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
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
        private readonly ITimeTableRepository _timeTableRepository;
        private readonly IGenericRepository<Category> _categoryRepo;
        private readonly IGenericRepository<SubCategory> _subCategoryRepo;
        private readonly IGenericRepository<Student> _studentRepo;
        private readonly IGenericRepository<TimeTable> _timeTableRepo;
        private readonly IMapper _mapper;
        private readonly JwtTokenHelper _jwtHelper;
        private readonly IConfiguration _configuration;



        public HomeController(ILogger<HomeController> logger, 
            ApplicationDbContext context, 
            IUserRepository userRepository, 
            ITeacherMasterRepository teacherMasterRepository, 
            IAdminRepository adminRepository, 
            IStudentRepository studentRepository, 
            IAttendanceRepository attendanceRepository, 
            ICategoryRepository categoryRepository, 
            ISubCategoryRepository subCategoryRepository, 
            ITimeTableRepository timeTableRepository,
            IGenericRepository<Category> categoryRepo, 
            IGenericRepository<SubCategory> subCategoryRepo, 
            IGenericRepository<Student> studentRepo, 
            IGenericRepository<TimeTable> timeTableRepo,
            IMapper mapper,
            JwtTokenHelper jwtHelper,
            IConfiguration configuration)
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
            _timeTableRepository = timeTableRepository;
            _categoryRepo = categoryRepo;
            _subCategoryRepo = subCategoryRepo;
            _studentRepo = studentRepo;
            _timeTableRepo = timeTableRepo;
            _mapper = mapper;
            _jwtHelper = jwtHelper;
            _configuration = configuration;
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

        private string GenerateJwtToken(User user)
        {
            var key = _configuration["Jwt:Key"];
            var issuer = _configuration["Jwt:Issuer"];
            var audience = _configuration["Jwt:Audience"];

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Email, user.Email),
        new Claim(ClaimTypes.Name, user.Name),
        new Claim(ClaimTypes.Role, user.Role)
    };

            if (user.StudentId.HasValue)
            {
                claims.Add(new Claim("StudentId", user.StudentId.Value.ToString()));
            }

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddSeconds(Convert.ToDouble(_configuration["Jwt:ExpireSeconds"])),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
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
            var token = GenerateJwtToken(user);
            HttpContext.Session.SetString("JWTToken", token);
            HttpContext.Session.SetString("TokenExpireTime", DateTime.UtcNow.AddSeconds(60).ToString("o"));

            HttpContext.Session.SetString("UserName", user.Name);
            HttpContext.Session.SetString("UserEmail", user.Email);
            HttpContext.Session.SetString("UserRole", user.Role);

            if (user.Role == "Student" && user.StudentId.HasValue)
                HttpContext.Session.SetInt32("StudentId", user.StudentId.Value);

            if (user.Role == "Student") return RedirectToAction("StudentDetails", "Home");
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


        // GET: Display list of categories
        [HttpGet]
        public IActionResult Category()
        {
            var categories = _categoryRepository.GetAllCategories();
            var viewModelList = _mapper.Map<List<CategoryViewModel>>(categories);
            return View(viewModelList);
        }

        // POST: Add a new category
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

        // GET: Edit form
        [HttpGet]
        public async Task<IActionResult> EditCategory(int id)
        {
            var category = await _categoryRepo.GetByIdAsync(id);
            if (category == null) return NotFound();

            var viewModel = _mapper.Map<CategoryViewModel>(category);
            return View(viewModel);
        }

        // POST: Edit submit
        [HttpPost]
        public async Task<IActionResult> EditCategory(CategoryViewModel model)
        {
            if (ModelState.IsValid)
            {
                var category = _mapper.Map<Category>(model);
                await _categoryRepo.UpdateAsync(category);
                await _categoryRepo.SaveAsync();
                return RedirectToAction("Category");
            }
            return View(model);
        }

        // DELETE
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


        // GET: Show all subcategories
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

        // POST: Add a new subcategory
        [HttpPost]
        public IActionResult Subcategories(SubCategoryViewModel model)
        {
            if (!string.IsNullOrEmpty(model.SubCategoryToEdit?.Name) && model.SubCategoryToEdit.CategoryId > 0)
            {
                var newSubCategory = _mapper.Map<SubCategory>(model.SubCategoryToEdit);
                _subCategoryRepository.AddSubCategory(newSubCategory);
            }

            var viewModel = new SubCategoryViewModel
            {
                Categories = _categoryRepository.GetCategorySelectList(),
                SubCategoryList = _subCategoryRepository.GetSubCategoriesWithCategory()
            };

            return View(viewModel);
        }

        // GET: Edit subcategory
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

        // POST: Edit subcategory
        [HttpPost]
        public async Task<IActionResult> EditSub(SubCategoryViewModel model)
        {
            if (ModelState.IsValid)
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

        // POST: Update directly from inline edit
        [HttpPost]
        public async Task<IActionResult> UpdateSub(int id, int CategoryId, string Name)
        {
            var sub = await _subCategoryRepo.GetByIdAsync(id);
            if (sub != null)
            {
                sub.CategoryId = CategoryId;
                sub.Name = Name;
                sub.CreatedDate = DateTime.Now;
                sub.CreatedBy = "admin";

                await _subCategoryRepo.UpdateAsync(sub);
                await _subCategoryRepo.SaveAsync();
            }

            return RedirectToAction("Subcategories");
        }

        // GET: Delete
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
        public JsonResult GetSubCategories(int categoryId)
        {
            var subCats = _context.SubCategories
                .Where(x => x.CategoryId == categoryId)
                .Select(x => new { x.Id, x.Name })
                .ToList();
            return Json(subCats);
        }


        [HttpGet]
        public IActionResult StudentDetails()
        {
            var studentId = HttpContext.Session.GetInt32("StudentId");
            if (studentId == null)
                return RedirectToAction("StudentDetails", "Home");

            var student = _context.Student.FirstOrDefault(s => s.Id == studentId.Value);
            if (student == null)
                return NotFound();

            // Optional: You can pass ViewModel instead if needed
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
        public IActionResult TimeTableForm()
        {
            string role = HttpContext.Session.GetString("UserRole");
            string email = HttpContext.Session.GetString("UserEmail");

            var viewModel = new TimeTableViewModel
            {
                Role = role,
                Message = TempData["Message"] as string,

                StdList = _categoryRepository.GetAllCategories()
                    .Select(c => new SelectListItem { Value = c.Name, Text = c.Name }).ToList(),

                ClassList = new List<SelectListItem>(),

                Hours = Enumerable.Range(1, 12)
                    .Select(h => new SelectListItem { Value = h.ToString(), Text = h.ToString() }).ToList(),

                Minutes = Enumerable.Range(0, 60)
                    .Select(m => new SelectListItem { Value = m.ToString("D2"), Text = m.ToString("D2") }).ToList(),

                TimeTableList = _timeTableRepository.GetAllTimeTables()
            };

            return View(viewModel);
        }


        // ✅ POST: TimeTableForm
        [HttpPost]
        public IActionResult TimeTableForm(TimeTableViewModel model)
        {
            if (ModelState.IsValid)
            {
                _timeTableRepository.AddTimeTable(model.TimeTable);
                TempData["Message"] = "Time Table Saved!";
                return RedirectToAction("TimeTableForm");
            }

            // ❌ If model is invalid, refill dropdowns
            model.StdList = _categoryRepository.GetAllCategories()
                .Select(c => new SelectListItem { Value = c.Name, Text = c.Name }).ToList();

            model.ClassList = _subCategoryRepository.GetAllSubCategories()
                .Select(sc => new SelectListItem { Value = sc.Name, Text = sc.Name }).ToList();

            model.Hours = Enumerable.Range(1, 24)
                .Select(i => new SelectListItem { Value = i.ToString(), Text = i.ToString() }).ToList();

            model.Minutes = Enumerable.Range(0, 59)
                .Select(i => new SelectListItem { Value = i.ToString("D2"), Text = i.ToString("D2") }).ToList();

            model.TimeTableList = _timeTableRepository.GetAllTimeTables();

            return View("TimeTableForm", model);
        }



        // ✅ AJAX: Get SubCategories by Standard
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
        public async Task<IActionResult> EditTimeTable(int id)
        {
            var record = await _timeTableRepo.GetByIdAsync(id);
            if (record == null)
                return NotFound();

            var viewModel = _mapper.Map<TimeTableViewModel>(record);

            viewModel.StdList = _context.Categories
                .Select(c => new SelectListItem { Value = c.Name, Text = c.Name }).ToList();

            viewModel.ClassList = _context.SubCategories
                .Select(sc => new SelectListItem { Value = sc.Name, Text = sc.Name }).ToList();

            viewModel.Hours = Enumerable.Range(1, 24)
                .Select(i => new SelectListItem { Value = i.ToString(), Text = i.ToString() }).ToList();

            viewModel.Minutes = Enumerable.Range(1, 60)
                .Select(i => new SelectListItem { Value = i.ToString(), Text = i.ToString() }).ToList();

            return View("UpdateTimeTable", viewModel);
        }


        [HttpPost]
        public async Task<IActionResult> EditTimeTable(TimeTableViewModel model)
        {
            if (ModelState.IsValid)
            {
                var entity = _mapper.Map<TimeTable>(model);
                await _timeTableRepo.UpdateAsync(entity);
                await _timeTableRepo.SaveAsync();

                TempData["Message"] = "Record updated successfully!";
                return RedirectToAction("TimeTableForm");
            }

            model.StdList = _context.Categories
                .Select(c => new SelectListItem { Value = c.Name, Text = c.Name }).ToList();

            model.ClassList = _context.SubCategories
                .Select(sc => new SelectListItem { Value = sc.Name, Text = sc.Name }).ToList();

            model.Hours = Enumerable.Range(1, 24)
                .Select(i => new SelectListItem { Value = i.ToString(), Text = i.ToString() }).ToList();

            model.Minutes = Enumerable.Range(1, 60)
                .Select(i => new SelectListItem { Value = i.ToString(), Text = i.ToString() }).ToList();

            return View("UpdateTimeTable", model);
        }


        public async Task<IActionResult> DeleteTimeTable(int id)
        {
            var record = await _timeTableRepo.GetByIdAsync(id);
            if (record != null)
            {
                await _timeTableRepo.DeleteAsync(record);
                await _timeTableRepo.SaveAsync();
                TempData["Message"] = "Record deleted successfully!";
            }
            return RedirectToAction("TimeTableForm");
        }




        public IActionResult AttendanceForm(int? CategoryId, int? SubCategoryId, int? Month, int? Year)
        {
            string role = HttpContext.Session.GetString("UserRole");
            var model = new AttendanceViewModel
            {
                SelectedCategoryId = CategoryId,
                SelectedSubCategoryId = SubCategoryId,
                SelectedMonth = Month,
                SelectedYear = Year,
                Categories = _context.Categories.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                }).ToList(),
                SubCategories = CategoryId.HasValue
                    ? _context.SubCategories.Where(sc => sc.CategoryId == CategoryId)
                        .Select(sc => new SelectListItem
                        {
                            Value = sc.Id.ToString(),
                            Text = sc.Name
                        }).ToList()
                    : new List<SelectListItem>()
            };

            if (role == "Student")
            {
                string email = HttpContext.Session.GetString("UserEmail");
                var student = _context.Student.FirstOrDefault(s => s.Email == email);

                if (student == null)
                    return RedirectToAction("Login");

                model.IsStudent = true;
                model.AttendanceData = _attendanceRepository.GetAttendanceByStudentId(student.Id);
                return View(model);
            }

            if (CategoryId.HasValue && SubCategoryId.HasValue && Month.HasValue && Year.HasValue)
            {
                var students = _context.Student
                    .Where(s => s.CategoryId == CategoryId && s.SubCategoryId == SubCategoryId)
                    .ToList();

                model.Students = students;
                model.TotalDays = DateTime.DaysInMonth(Year.Value, Month.Value);
                model.AttendanceData = _attendanceRepository.GetAttendanceByFilters(students.Select(s => s.Id).ToList(), Month.Value, Year.Value);
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
                .Where(x => x.CategoryId == catId)
                .Select(sc => new SelectListItem
                {
                    Value = sc.Id.ToString(),
                    Text = sc.Name
                }).ToList();

            model.Students = students;
            model.TotalDays = totalDays;
            model.AttendanceData = _attendanceRepository.GetAttendanceByFilters(students.Select(s => s.Id).ToList(), month, year);

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

        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Home");
        }

        public IActionResult TeacherMasterPage()
        {
            ViewBag.Categories = _context.Categories.ToList();
            ViewBag.Subjects = _context.Subjects.ToList();
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
            ViewBag.Subjects = _context.Subjects.ToList();
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