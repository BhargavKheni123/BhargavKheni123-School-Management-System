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

            var subjectExam = (from q in _context.QuestionMaster
                               join s in _context.Subjects on q.SubjectId equals s.Id
                               where q.CategoryId == student.CategoryId
                               select new { s.Name, q.ExamType })
                              .FirstOrDefault();

            if (subjectExam != null)
            {
                ViewBag.SubjectName = subjectExam.Name;
                ViewBag.ExamType = subjectExam.ExamType;
            }

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

                var question = _context.QuestionMaster.FirstOrDefault(q => q.Id == ans.QuestionId);
                if (question != null)
                {
                    ans.SubjectId = question.SubjectId;
                    ans.ExamType = question.ExamType;
                }
            }
        
        _context.StudentAnswers.AddRange(answers);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        public IActionResult ThankYou()
        {
            return View();
        }
    }
}
