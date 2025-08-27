using ClosedXML.Excel;
using digital.Interfaces;
using digital.Models;
using digital.Repositories;
using digital.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace digital.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IUserRepository _userRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly ITeacherRepository _teacherRepository;
        private readonly IConfiguration _configuration;
        private readonly IAttendanceRepository _attendanceRepository;
        private readonly IExamRepository _examRepository;

        public HomeController(ILogger<HomeController> logger,
                              ApplicationDbContext context,
                              IUserRepository userRepository,
                              IStudentRepository studentRepository,
                              ITeacherRepository teacherRepository,
                              IConfiguration configuration,
                              IAttendanceRepository attendanceRepository,
                              IExamRepository examRepository)
        {
            _logger = logger;
            _context = context;
            _userRepository = userRepository;
            _studentRepository = studentRepository;
            _teacherRepository = teacherRepository;
            _configuration = configuration;
            _attendanceRepository = attendanceRepository;
            _examRepository = examRepository;
        }

        #region JWT Helper
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
                claims.Add(new Claim("StudentId", user.StudentId.Value.ToString()));
            if (user.TeacherId.HasValue)
                claims.Add(new Claim("TeacherId", user.TeacherId.Value.ToString()));

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddSeconds(Convert.ToDouble(_configuration["Jwt:ExpireSeconds"])),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateNameFromEmail(string email)
        {
            var usernamePart = email.Split('@')[0];
            var parts = System.Text.RegularExpressions.Regex
                            .Split(usernamePart, @"(?<!^)(?=[A-Z])|[_\\.\-\\s]+");

            if (parts.Length == 1)
                return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(parts[0]);

            return string.Join(" ", parts.Select(p =>
                System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(p.ToLower())));
        }
        #endregion

        #region Auth
        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            var user = _userRepository.GetUserByEmailAndPassword(email, password);
            if (user == null)
            {
                ViewBag.Error = "Invalid email or password.";
                return View();
            }

            if (user.IsLoggedIn)
            {
                ViewBag.Error = "User already logged in from another device. Please logout first.";
                return View();
            }

            var newSessionId = Guid.NewGuid().ToString();
            user.IsLoggedIn = true;
            user.CurrentSessionId = newSessionId;

            _userRepository.UpdateUser(user);

            var token = GenerateJwtToken(user);
            HttpContext.Session.SetString("JWTToken", token);
            HttpContext.Session.SetString("TokenExpireTime", DateTime.UtcNow.AddMinutes(15).ToString("o"));
            HttpContext.Session.SetString("SessionId", newSessionId);
            HttpContext.Session.SetString("UserName", user.Name);
            HttpContext.Session.SetString("UserEmail", user.Email);
            HttpContext.Session.SetString("UserRole", user.Role);

            if (user.Role == "Student" && user.StudentId.HasValue)
                HttpContext.Session.SetInt32("StudentId", user.StudentId.Value);
            if (user.Role == "Teacher" && user.TeacherId.HasValue)
                HttpContext.Session.SetInt32("TeacherId", user.TeacherId.Value);

            if (user.Role == "Student") return RedirectToAction("StudentDetails", "Student");
            if (user.Role == "Admin") return RedirectToAction("Index", "Home");
            if (user.Role == "Teacher") return RedirectToAction("TimeTableForm", "TimeTable");

            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult Register()
        {
            var model = new RegisterViewModel
            {
                UserList = _userRepository.GetAllUsers().ToList()
            };
            return View(model);
        }

        [HttpPost]
        public IActionResult Register(RegisterViewModel model)
        {
            var existingUser = _userRepository.GetUserByEmail(model.Email);
            if (existingUser != null)
            {
                model.UserList = _userRepository.GetAllUsers().ToList();
                ViewBag.Error = "User already exists with this email.";
                return View(model);
            }

            var newUser = new User
            {
                Name = model.Name,
                Email = model.Email,
                Password = model.Password,
                Role = model.Role,
                CurrentSessionId = null
            };

            _userRepository.AddUser(newUser);

            if (model.Role == "Student")
            {
                var student = new Student
                {
                    Name = newUser.Name,
                    Email = model.Email,
                    Password = model.Password,
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

                return RedirectToAction("Index", "Student");
            }

            return RedirectToAction("Login");
        }


        [HttpPost]
        public IActionResult Logout()
        {
            var email = HttpContext.Session.GetString("UserEmail");
            if (!string.IsNullOrEmpty(email))
            {
                var user = _userRepository.GetUserByEmail(email);
                if (user != null)
                {
                    user.IsLoggedIn = false;
                    user.CurrentSessionId = null;

                    _userRepository.UpdateUser(user);
                }
            }

            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Home");
        }


        [HttpPost]
        public IActionResult ImportFromExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["Error"] = "Please select a valid Excel file.";
                return RedirectToAction("Register");
            }

            using (var stream = new MemoryStream())
            {
                file.CopyTo(stream);
                using (var workbook = new XLWorkbook(stream))
                {
                    var worksheet = workbook.Worksheet("Users"); 
                    var rows = worksheet.RowsUsed().Skip(1); 

                    foreach (var row in rows)
                    {
                        var name = row.Cell(2).GetValue<string>();
                        var email = row.Cell(3).GetValue<string>();
                        var role = row.Cell(4).GetValue<string>();

                        var existingUser = _userRepository.GetUserByEmail(email);
                        if (existingUser != null) continue;

                        var user = new User
                        {
                            Name = name,
                            Email = email,
                            Password = "12345", 
                            Role = role
                        };

                        _userRepository.AddUser(user);

                        if (role == "Student")
                        {
                            var student = new Student
                            {
                                Name = name,
                                Email = email,
                                Password = "12345",
                                CreatedDate = DateTime.Now,
                                CategoryId = 1,
                                SubCategoryId = 1,
                                DOB = DateTime.Now,
                                Gender = "Not Set",
                                MobileNumber = "0000000000",
                                Address = "Not Provided"
                            };

                            _studentRepository.AddStudent(student);
                        }
                    }
                }
            }

            TempData["Success"] = "Users imported successfully.";
            return RedirectToAction("Register");
        }

        #endregion
        public IActionResult ExportToExcel()
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Users");

            worksheet.Cell(1, 1).Value = "No.";
            worksheet.Cell(1, 2).Value = "Name";
            worksheet.Cell(1, 3).Value = "Email";
            worksheet.Cell(1, 4).Value = "Role";
            worksheet.Cell(1, 5).Value = "DOB";
            worksheet.Cell(1, 6).Value = "Gender";
            worksheet.Cell(1, 7).Value = "Class";
            worksheet.Cell(1, 8).Value = "Division";
            worksheet.Cell(1, 9).Value = "Mobile";
            worksheet.Cell(1, 10).Value = "Address";

            var students = _studentRepository.GetAllStudentsWithCategoryAndSubCategory();
            var teachers = _teacherRepository.GetAllTeachers().ToList();
            var users = _userRepository.GetAllUsers();

            int row = 2;
            int counter = 1;

            foreach (var u in users)
            {
                worksheet.Cell(row, 1).Value = counter++;
                worksheet.Cell(row, 2).Value = u.Name;
                worksheet.Cell(row, 3).Value = u.Email;
                worksheet.Cell(row, 4).Value = u.Role;

                if (u.Role == "Student" && u.StudentId.HasValue)
                {
                    var s = students.FirstOrDefault(x => x.Id == u.StudentId.Value);
                    if (s != null)
                    {
                        //worksheet.Cell(row, 5).Value = s.DOB.ToString("yyyy-MM-dd");
                        //worksheet.Cell(row, 6).Value = s.Gender;
                        worksheet.Cell(row, 7).Value = s.Category?.Name ?? s.CategoryId.ToString();
                        worksheet.Cell(row, 8).Value = s.SubCategory?.Name ?? s.SubCategoryId.ToString();
                        worksheet.Cell(row, 9).Value = s.MobileNumber;
                        //worksheet.Cell(row, 10).Value = s.Address;
                    }
                }
                else if (u.Role == "Teacher" && u.TeacherId.HasValue)
                {
                    var t = teachers.FirstOrDefault(x => x.TeacherId == u.TeacherId.Value);
                    if (t != null)
                    {
                        worksheet.Cell(row, 5).Value = t.DOB.ToString("yyyy-MM-dd");
                        worksheet.Cell(row, 6).Value = t.Gender;
                    }
                    worksheet.Cell(row, 7).Value = "";
                    worksheet.Cell(row, 8).Value = "";
                    worksheet.Cell(row, 9).Value = "";
                    worksheet.Cell(row, 10).Value = "";
                }

                row++;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var content = stream.ToArray();
            return File(content,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "Users.xlsx"
            );
        }


        [HttpGet]
        public IActionResult ExportToPdf()
        {
            var students = _studentRepository.GetAllStudentsWithCategoryAndSubCategory();
            var teachers = _teacherRepository.GetAllTeachers().ToList();
            var users = _userRepository.GetAllUsers();

            int counter = 1;

            var fileName = $"Users_{DateTime.Now:yyyyMMdd_HHmm}.pdf";

            var pdfBytes = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(20);


                    page.Header().Column(col =>
                    {
                        col.Spacing(5);
                        col.Item().AlignCenter().Text("Users Report")
                            .FontSize(18).SemiBold();
                        col.Item().LineHorizontal(1);
                    });


                    page.Content().Table(table =>
                    {

                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(1);  // No.
                            columns.RelativeColumn(3);  // Name
                            columns.RelativeColumn(3);  // Email
                            columns.RelativeColumn(2);  // Role
                            columns.RelativeColumn(2);  // DOB
                            columns.RelativeColumn(2);  // Gender
                            columns.RelativeColumn(2);  // Class
                            columns.RelativeColumn(2);  // Division
                            columns.RelativeColumn(3);  // Mobile
                            columns.RelativeColumn(4);  // Address
                        });


                        void HeaderCell(string text) =>
                            table.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text(text).SemiBold();

                        HeaderCell("No.");
                        HeaderCell("Name");
                        HeaderCell("Email");
                        HeaderCell("Role");
                        HeaderCell("DOB");
                        HeaderCell("Gender");
                        HeaderCell("Class");
                        HeaderCell("Division");
                        HeaderCell("Mobile");
                        HeaderCell("Address");


                        foreach (var u in users)
                        {
                            table.Cell().Padding(3).Text(counter++.ToString());
                            table.Cell().Padding(3).Text(u.Name ?? "");
                            table.Cell().Padding(3).Text(u.Email ?? "");
                            table.Cell().Padding(3).Text(u.Role ?? "");

                            string dob = "";
                            string gender = "";
                            string className = "";
                            string division = "";
                            string mobile = "";
                            string address = "";

                            if (u.Role == "Student" && u.StudentId.HasValue)
                            {
                                var s = students.FirstOrDefault(x => x.Id == u.StudentId.Value);
                                if (s != null)
                                {
                                    //dob = s.DOB != default ? s.DOB.ToString("yyyy-MM-dd") : "";
                                    //gender = s.Gender ?? "";
                                    className = s.Category?.Name ?? s.CategoryId.ToString();
                                    division = s.SubCategory?.Name ?? s.SubCategoryId.ToString();
                                    mobile = s.MobileNumber ?? "";
                                    //address = s.Address ?? "";
                                }
                            }
                            else if (u.Role == "Teacher" && u.TeacherId.HasValue)
                            {
                                var t = teachers.FirstOrDefault(x => x.TeacherId == u.TeacherId.Value);
                                if (t != null)
                                {
                                    dob = t.DOB != default ? t.DOB.ToString("yyyy-MM-dd") : "";
                                    gender = t.Gender ?? "";
                                }
                            }

                            table.Cell().Padding(3).Text(dob);
                            table.Cell().Padding(3).Text(gender);
                            table.Cell().Padding(3).Text(className);
                            table.Cell().Padding(3).Text(division);
                            table.Cell().Padding(3).Text(mobile);
                            table.Cell().Padding(3).Text(address);
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




        public IActionResult Index()
        {
            var email = HttpContext.Session.GetString("UserEmail");
            ViewBag.UserName = string.IsNullOrEmpty(email) ? "Email not in session" : $"Welcome, {email}";

            ViewBag.TotalStudents = _studentRepository.GetAllStudentsWithCategoryAndSubCategory().Count;

            ViewBag.TotalTeachers = _teacherRepository.GetAllTeachers().Count();

            var allAttendance = _context.Attendance.ToList();
            if (allAttendance.Count > 0)
            {
                int presentCount = allAttendance.Count(a => a.Attend == "Yes");
                double percentage = (double)presentCount / allAttendance.Count * 100;
                ViewBag.AttendancePercentage = Math.Round(percentage, 2);
            }
            else
            {
                ViewBag.AttendancePercentage = 0;
            }

            var upcomingExam = _examRepository.GetAllExams()
                .Where(e => e.ExamDate != null && e.ExamDate >= DateTime.Now)
                .OrderBy(e => e.ExamDate)
                .FirstOrDefault();

            if (upcomingExam != null)
            {
                ViewBag.UpcomingExamTitle = upcomingExam.ExamTitle;
                ViewBag.UpcomingExamDate = upcomingExam.ExamDate?.ToString("dd MMM yyyy");
            }
            else
            {
                ViewBag.UpcomingExamTitle = "No Upcoming Exam";
                ViewBag.UpcomingExamDate = "";
            }

            return View();
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}