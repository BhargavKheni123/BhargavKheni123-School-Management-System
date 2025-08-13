using digital.Interfaces;
using digital.Models;
using digital.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
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
        private readonly IConfiguration _configuration;

        public HomeController(ILogger<HomeController> logger,
                              ApplicationDbContext context,
            IUserRepository userRepository,
            IStudentRepository studentRepository,
            IConfiguration configuration)
        {
            _logger = logger;
            _context = context;
            _userRepository = userRepository;
            _studentRepository = studentRepository;
            _configuration = configuration;

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
                            .Split(usernamePart, @"(?<!^)(?=[A-Z])|[_\.\-\s]+");

            if (parts.Length == 1)
                return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(parts[0]);

            return string.Join(" ", parts.Select(p =>
                System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(p.ToLower())));
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
            HttpContext.Session.SetString("TokenExpireTime", DateTime.UtcNow.AddMinutes(15).ToString("o"));

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
            var users = _userRepository.GetAllUsers(); 
            ViewBag.UserList = users;
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
                Name = string.IsNullOrWhiteSpace(name) ? GenerateNameFromEmail(email) : name,
                Email = email,
                Password = password,
                Role = role
            };

            _userRepository.AddUser(newUser);

            if (role == "Student")
            {
                var student = new Student
                {
                    Name = newUser.Name,
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

                return RedirectToAction("Index", "Student");
            }
            ViewBag.UserList = _userRepository.GetAllUsers();
            return RedirectToAction("Login");
        }

        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Home");
        }
    
        public IActionResult Index()
        {
            var email = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(email))
            {
                ViewBag.UserName = "Email not in session";
                return View();
            }

            ViewBag.UserName = $"Welcome, {email}";
            return View();
        }

        

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
