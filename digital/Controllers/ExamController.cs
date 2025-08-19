using digital.Models;
using digital.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace digital.Controllers
{
    public class ExamController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ExamController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult CreateExam()
        {
            var loggedInUserEmail = User.Identity?.Name ?? HttpContext.Session.GetString("UserEmail");
            var loggedInTeacherId = _context.Teachers
                .Where(u => u.Email == loggedInUserEmail)
                .Select(u => u.TeacherId)
                .FirstOrDefault();

            var model = new ExamViewModel
            {
                Categories = _context.Categories
                    .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
                    .ToList(),
                Subjects = _context.Subjects
                    .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name })
                    .ToList(),
                Teachers = _context.Users
                    .Where(u => u.Role == "Teacher")
                    .Select(t => new SelectListItem { Value = t.Id.ToString(), Text = t.Name })
                    .ToList()
            };

            // ✅ Include navigation props and map safely
            var examQuery = _context.Exams
                .Include(e => e.Category)
                .Include(e => e.Subject)
                .Include(e => e.AssignedTeacher)
                .Select(e => new ExamListItem
                {
                    ExamId = e.ExamId,
                    ExamTitle = e.ExamTitle,
                    ExamType = e.ExamType,
                    ClassName = e.Category != null ? e.Category.Name : "N/A",
                    SubjectName = e.Subject != null ? e.Subject.Name : "N/A",
                    TeacherName = e.AssignedTeacher != null ? e.AssignedTeacher.Name : "Unassigned",
                    AssignedTeacherId = e.AssignedTeacherId,
                    ExamDate = e.ExamDate,
                    Description = e.Description
                });

            if (User.IsInRole("Teacher"))
            {
                examQuery = examQuery.Where(e => e.AssignedTeacherId == loggedInTeacherId);
            }

            model.ExamList = examQuery.ToList();
            return View(model);
        }

        [HttpPost]
        public IActionResult CreateExam(ExamViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // re-populate dropdowns
                model.Categories = _context.Categories
                    .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
                    .ToList();
                model.Subjects = _context.Subjects
                    .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name })
                    .ToList();
                model.Teachers = _context.Users
                    .Where(u => u.Role == "Teacher")
                    .Select(t => new SelectListItem { Value = t.Id.ToString(), Text = t.Name })
                    .ToList();

                // keep the list visible on validation errors
                model.ExamList = _context.Exams
                    .Include(e => e.Category)
                    .Include(e => e.Subject)
                    .Include(e => e.AssignedTeacher)
                    .Select(e => new ExamListItem
                    {
                        ExamId = e.ExamId,
                        ExamTitle = e.ExamTitle,
                        ExamType = e.ExamType,
                        ClassName = e.Category != null ? e.Category.Name : "N/A",
                        SubjectName = e.Subject != null ? e.Subject.Name : "N/A",
                        TeacherName = e.AssignedTeacher != null ? e.AssignedTeacher.Name : "Unassigned",
                        AssignedTeacherId = e.AssignedTeacherId,
                        ExamDate = e.ExamDate,
                        Description = e.Description
                    }).ToList();

                return View(model);
            }

            var exam = new Exam
            {
                ExamTitle = model.ExamTitle,
                Description = model.Description,
                ExamType = model.ExamType,
                CategoryId = model.CategoryId.GetValueOrDefault(),   // form requires it
                SubjectId = model.SubjectId.GetValueOrDefault(),     // form requires it
                AssignedTeacherId = model.AssignedTeacherId,         // nullable
                ExamDate = model.ExamDate,                           // nullable
                CreatedBy = 1,
                CreatedDate = DateTime.Now
            };

            _context.Exams.Add(exam);
            _context.SaveChanges();

            TempData["Success"] = "Exam created successfully!";
            return RedirectToAction("CreateExam");
        }

        [HttpGet]
        public IActionResult EditExam(int id)
        {
            var exam = _context.Exams.FirstOrDefault(e => e.ExamId == id);
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

                Categories = _context.Categories
                    .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
                    .ToList(),
                Subjects = _context.Subjects
                    .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name })
                    .ToList(),
                Teachers = _context.Users
                    .Where(u => u.Role == "Teacher")
                    .Select(t => new SelectListItem { Value = t.Id.ToString(), Text = t.Name })
                    .ToList(),
                ExamList = GetExamList()
            };

            return View("CreateExam", model);
        }

        [HttpPost]
        public IActionResult EditExam(ExamViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Categories = _context.Categories
                    .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
                    .ToList();
                model.Subjects = _context.Subjects
                    .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name })
                    .ToList();
                model.Teachers = _context.Users
                    .Where(u => u.Role == "Teacher")
                    .Select(t => new SelectListItem { Value = t.Id.ToString(), Text = t.Name })
                    .ToList();
                model.ExamList = GetExamList();
                return View("CreateExam", model);
            }

            var exam = _context.Exams.FirstOrDefault(e => e.ExamId == model.ExamId);
            if (exam != null)
            {
                exam.ExamTitle = model.ExamTitle;
                exam.Description = model.Description;
                exam.ExamType = model.ExamType;
                exam.CategoryId = model.CategoryId.GetValueOrDefault();
                exam.SubjectId = model.SubjectId.GetValueOrDefault();
                exam.AssignedTeacherId = model.AssignedTeacherId; // nullable ok
                exam.ExamDate = model.ExamDate;                   // nullable ok
            }

            _context.SaveChanges();
            return RedirectToAction("CreateExam");
        }

        [HttpGet]
        public IActionResult DeleteExam(int id)
        {
            var exam = _context.Exams.FirstOrDefault(e => e.ExamId == id);
            if (exam != null)
            {
                _context.Exams.Remove(exam);
                _context.SaveChanges();
            }
            return RedirectToAction("CreateExam");
        }

        private List<ExamListItem> GetExamList(int? teacherId = null)
        {
            var query = _context.Exams
                .Include(e => e.Category)
                .Include(e => e.Subject)
                .Include(e => e.AssignedTeacher)
                .Select(e => new ExamListItem
                {
                    ExamId = e.ExamId,
                    ExamTitle = e.ExamTitle,
                    ExamType = e.ExamType,
                    ClassName = e.Category != null ? e.Category.Name : "N/A",
                    SubjectName = e.Subject != null ? e.Subject.Name : "N/A",
                    TeacherName = e.AssignedTeacher != null ? e.AssignedTeacher.Name : "Unassigned",
                    AssignedTeacherId = e.AssignedTeacherId,
                    ExamDate = e.ExamDate,
                    Description = e.Description
                });

            if (teacherId.HasValue)
            {
                query = query.Where(e => e.AssignedTeacherId == teacherId.Value);
            }

            return query.ToList();
        }

        [HttpGet]
        public IActionResult StudentExamList()
        {
            var studentId = HttpContext.Session.GetInt32("StudentId");
            if (studentId == null)
                return RedirectToAction("StudentDetails", "Student");

            var student = _context.Student.FirstOrDefault(s => s.Id == studentId.Value);
            if (student == null)
                return NotFound();

            var exams = _context.Exams
                .Where(e => e.CategoryId == student.CategoryId)
                .Include(e => e.Category)
                .Include(e => e.Subject)
                .Include(e => e.AssignedTeacher)
                .Select(e => new ExamListItem
                {
                    ExamId = e.ExamId,
                    ExamTitle = e.ExamTitle,
                    ExamType = e.ExamType,
                    ClassName = e.Category != null ? e.Category.Name : "N/A",
                    SubjectName = e.Subject != null ? e.Subject.Name : "N/A",
                    TeacherName = e.AssignedTeacher != null ? e.AssignedTeacher.Name : "Unassigned",
                    AssignedTeacherId = e.AssignedTeacherId,
                    ExamDate = e.ExamDate,
                    Description = e.Description
                })
                .ToList();

            var model = new ExamViewModel { ExamList = exams };
            return View(model);
        }

        [HttpGet]
        public IActionResult TeacherExamList()
        {
            var loggedInUserEmail = User.Identity?.Name ?? HttpContext.Session.GetString("UserEmail");

            var loggedInTeacherId = _context.Teachers
                .Where(t => t.Email == loggedInUserEmail)
                .Select(t => t.TeacherId)
                .FirstOrDefault();

            if (loggedInTeacherId == 0)
            {
                ViewBag.Message = "No exams found for this teacher.";
                return View(new ExamViewModel { ExamList = new List<ExamListItem>() });
            }

            var exams = _context.Exams
                .Include(e => e.Category)
                .Include(e => e.Subject)
                .Include(e => e.AssignedTeacher)
                .Where(e => e.AssignedTeacherId == loggedInTeacherId)
                .Select(e => new ExamListItem
                {
                    ExamId = e.ExamId,
                    ExamTitle = e.ExamTitle,
                    ExamType = e.ExamType,
                    Description = e.Description,
                    ClassName = e.Category != null ? e.Category.Name : "N/A",
                    SubjectName = e.Subject != null ? e.Subject.Name : "N/A",
                    TeacherName = e.AssignedTeacher != null ? e.AssignedTeacher.Name : "Unassigned",
                    ExamDate = e.ExamDate
                })
                .ToList();

            var model = new ExamViewModel { ExamList = exams };
            return View(model);
        }
    }
}
