using AutoMapper;
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
            var loggedInUserId = _context.Users
                .Where(u => u.Email == loggedInUserEmail)
                .Select(u => u.Id)
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

            var examQuery = _context.Exams
                .Select(e => new ExamListItem
                {
                    ExamId = e.ExamId,
                    ExamTitle = e.ExamTitle,
                    ExamType = e.ExamType,
                    ClassName = e.Category.Name,
                    SubjectName = e.Subject.Name,
                    TeacherName = e.AssignedTeacher.Name,
                    AssignedTeacherId = e.AssignedTeacherId,
                    ExamDate = e.ExamDate
                });

            if (User.IsInRole("Teacher"))
            {
                examQuery = examQuery.Where(e => e.AssignedTeacherId == loggedInUserId);
            }

            model.ExamList = examQuery.ToList();
            return View(model);
        }

        [HttpPost]
        public IActionResult CreateExam(ExamViewModel model)
        {
            var exam = new Exam
            {
                ExamTitle = model.ExamTitle,
                Description = model.Description,
                ExamType = model.ExamType,
                CategoryId = model.CategoryId,
                SubjectId = model.SubjectId,
                AssignedTeacherId = model.AssignedTeacherId,
                ExamDate = model.ExamDate,
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
                exam.CategoryId = model.CategoryId;
                exam.SubjectId = model.SubjectId;
                exam.AssignedTeacherId = model.AssignedTeacherId;
                exam.ExamDate = model.ExamDate;
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
                .Join(_context.Categories, e => e.CategoryId, c => c.Id, (e, c) => new { e, c })
                .Join(_context.Subjects, ecs => ecs.e.SubjectId, s => s.Id, (ecs, s) => new { ecs.e, ecs.c, s })
                .Join(_context.Users, ecss => ecss.e.AssignedTeacherId, u => u.Id, (ecss, u) => new ExamListItem
                {
                    ExamId = ecss.e.ExamId,
                    ExamTitle = ecss.e.ExamTitle,
                    ExamType = ecss.e.ExamType,
                    ClassName = ecss.c.Name,
                    SubjectName = ecss.s.Name,
                    TeacherName = u.Name,
                    AssignedTeacherId = ecss.e.AssignedTeacherId,
                    ExamDate = ecss.e.ExamDate
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
                .Join(_context.Categories, e => e.CategoryId, c => c.Id, (e, c) => new { e, c })
                .Join(_context.Subjects, ecs => ecs.e.SubjectId, s => s.Id, (ecs, s) => new { ecs.e, ecs.c, s })
                .Join(_context.Users, ecss => ecss.e.AssignedTeacherId, u => u.Id, (ecss, u) => new ExamListItem
                {
                    ExamId = ecss.e.ExamId,
                    ExamTitle = ecss.e.ExamTitle,
                    ExamType = ecss.e.ExamType,
                    ClassName = ecss.c.Name,
                    SubjectName = ecss.s.Name,
                    TeacherName = u.Name,
                    AssignedTeacherId = ecss.e.AssignedTeacherId,
                    ExamDate = ecss.e.ExamDate
                })
                .ToList();

            var model = new ExamViewModel { ExamList = exams };
            return View(model);
        }

        [HttpGet]
        public IActionResult TeacherExamList()
        {
            var teacherId = HttpContext.Session.GetInt32("TeacherId");
            if (teacherId == null)
                return RedirectToAction("Login", "Account");

            var exams = _context.Exams
                .Where(e => e.AssignedTeacherId == teacherId.Value)
                .Select(e => new ExamListItem
                {
                    ExamId = e.ExamId,
                    ExamTitle = e.ExamTitle,
                    ExamType = e.ExamType,
                    ClassName = e.Category.Name,
                    SubjectName = e.Subject.Name,
                    TeacherName = e.AssignedTeacher.Name,
                    ExamDate = e.ExamDate
                })
                .ToList();

            return View(exams);
        }
    }
}
