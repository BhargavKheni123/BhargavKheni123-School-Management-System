using digital.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace digital.Controllers
{
    public class StudentExamController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StudentExamController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var studentId = HttpContext.Session.GetInt32("StudentId");
            if (studentId == null) return RedirectToAction("Login", "Account");

            var student = _context.Student
                .Include(s => s.Category)
                .FirstOrDefault(s => s.Id == studentId);

            if (student == null) return NotFound();

            var questions = _context.QuestionMaster
                .Include(q => q.AnswerOptions)
                .Include(q => q.Category)
                .Include(q => q.Subject)
                .Where(q => q.CategoryId == student.CategoryId)
                .ToList();

            ViewBag.StudentName = student.Name;
            ViewBag.Std = student.Category.Name;

            return View(questions);
        }

        [HttpPost]
        public IActionResult SubmitExam(List<StudentAnswer> answers)
        {
            var studentId = HttpContext.Session.GetInt32("StudentId");
            if (studentId == null) return RedirectToAction("Login", "Account");

            foreach (var ans in answers)
            {
                ans.StudentId = studentId.Value;
                ans.SubmittedOn = DateTime.Now;
            }

            _context.StudentAnswers.AddRange(answers);
            _context.SaveChanges();

            return RedirectToAction("ThankYou");
        }

        public IActionResult ThankYou()
        {
            return View();
        }
    }
}
