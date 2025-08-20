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
    .Where(q => q.CategoryId == student.CategoryId
         && q.ExamDate.HasValue
         && q.ExamDate.Value.Date == DateTime.Today)
    .ToList();


            ViewBag.StudentName = student.Name;
            ViewBag.Std = student.Category.Name;

            var subjectExam = (from q in _context.QuestionMaster
                               join s in _context.Subjects on q.SubjectId equals s.Id
                               where q.CategoryId == student.CategoryId
                               && q.ExamDate.HasValue
                               && q.ExamDate.Value.Date == DateTime.Today
                               select new
                               {
                                   s.Name,
                                   q.ExamType,
                                   q.ExamDate,
                                   q.StartHour,
                                   q.StartMinute,
                                   q.EndHour,
                                   q.EndMinute
                               }).FirstOrDefault();

            if (subjectExam != null)
            {
                ViewBag.SubjectName = subjectExam.Name;
                ViewBag.ExamType = subjectExam.ExamType;
                ViewBag.ExamDate = subjectExam.ExamDate?.ToString("dd-MM-yyyy");

                DateTime examStart = subjectExam.ExamDate.Value
                    .AddHours(subjectExam.StartHour ?? 0)
                    .AddMinutes(subjectExam.StartMinute ?? 0);

                DateTime examEnd = subjectExam.ExamDate.Value
                    .AddHours(subjectExam.EndHour ?? 23)
                    .AddMinutes(subjectExam.EndMinute ?? 59);

                ViewBag.ExamStart = examStart;
                ViewBag.ExamEnd = examEnd;

                var now = DateTime.Now;

                if (now < examStart)
                {
                    ViewBag.NoExamMsg = $"Exam will start at {examStart:HH:mm}. Please wait.";
                    questions.Clear();
                }
                else if (now > examEnd)
                {
                    ViewBag.NoExamMsg = "Exam time is over. You cannot attempt this exam.";
                    questions.Clear();
                }
            }

            if (!questions.Any() && ViewBag.NoExamMsg == null)
            {
                ViewBag.NoExamMsg = "No exam is available for today.";
            }

            return View(questions);
        }

        [HttpPost]
        public IActionResult SubmitExam(List<StudentAnswer> answers)
        {
            var studentId = HttpContext.Session.GetInt32("StudentId");
            if (studentId == null) return RedirectToAction("Login", "Account");

            
            var lastExam = _context.QuestionMaster
                .Where(q => q.ExamDate.HasValue && q.ExamDate.Value.Date == DateTime.Today)
                .OrderByDescending(q => q.ExamDate)
                .FirstOrDefault();

            if (lastExam != null)
            {
                DateTime examEnd = lastExam.ExamDate.Value
                    .AddHours(lastExam.EndHour ?? 23)
                    .AddMinutes(lastExam.EndMinute ?? 59);

                if (DateTime.Now > examEnd)
                {
                    return RedirectToAction("ThankYou"); 
                }
            }

            int correctCount = 0;
            int subjectId = 0;
            string examType = "";

            foreach (var ans in answers)
            {
                ans.StudentId = studentId.Value;
                ans.SubmittedOn = DateTime.Now;

                var question = _context.QuestionMaster
                                       .Include(q => q.AnswerOptions)
                                       .FirstOrDefault(q => q.Id == ans.QuestionId);

                if (question != null)
                {
                    ans.SubjectId = question.SubjectId;
                    ans.ExamType = question.ExamType;

                    subjectId = question.SubjectId;
                    examType = question.ExamType;

                    var correctOption = question.AnswerOptions.FirstOrDefault(o => o.IsCorrect);
                    if (correctOption != null && correctOption.OptionText == ans.SelectedAnswer)
                    {
                        correctCount++;
                    }
                }
            }

            var result = new StudentExamResult
            {
                StudentId = studentId.Value,
                SubjectId = subjectId,
                ExamType = examType,
                TotalQuestions = answers.Count,
                CorrectAnswers = correctCount,
                SubmittedOn = DateTime.Now
            };

            _context.StudentExamResults.Add(result);
            _context.SaveChanges();

            foreach (var ans in answers)
            {
                ans.ResultId = result.Id;
            }

            _context.StudentAnswers.AddRange(answers);
            _context.SaveChanges();

            return RedirectToAction("Result", new { resultId = result.Id });
        }

        public IActionResult Result(int resultId)
        {
            var studentId = HttpContext.Session.GetInt32("StudentId");
            if (studentId == null) return RedirectToAction("Login", "Account");

            var result = _context.StudentExamResults
                .Include(r => r.Subject)
                .Include(r => r.Student)
                .ThenInclude(s => s.Category)
                .FirstOrDefault(r => r.Id == resultId && r.StudentId == studentId);

            if (result == null) return NotFound();

            var answers = _context.StudentAnswers
                .Include(a => a.Question)
                .ThenInclude(q => q.AnswerOptions)
                .Where(a => a.ResultId == result.Id)
                .ToList();

            var vm = new ExamResultViewModel
            {
                Result = result,
                Answers = answers
            };

            return View(vm);
        }

        public IActionResult ThankYou()
        {
            return View();
        }
    }
}
