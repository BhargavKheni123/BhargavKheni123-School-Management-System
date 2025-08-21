using digital.Models;
using digital.ViewModels;
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

            ViewBag.StudentName = student.Name;
            ViewBag.Std = student.Category.Name;

            
            var exams = (from q in _context.QuestionMaster
                         join s in _context.Subjects on q.SubjectId equals s.Id
                         where q.CategoryId == student.CategoryId
                         && q.ExamDate.HasValue
                         && q.ExamDate.Value.Date == DateTime.Today
                         select new
                         {
                             SubjectName = s.Name,
                             q.SubjectId,
                             q.ExamType,
                             q.ExamDate,
                             q.StartHour,
                             q.StartMinute,
                             q.EndHour,
                             q.EndMinute
                         }).Distinct().ToList();

            if (!exams.Any())
            {
                ViewBag.NoExamMsg = "No exam is available for today.";
                return View(new List<QuestionMaster>());
            }

            
            var examData = new List<dynamic>();
            foreach (var exam in exams)
            {
                var questions = _context.QuestionMaster
                    .Include(q => q.AnswerOptions)
                    .Where(q => q.CategoryId == student.CategoryId
                             && q.SubjectId == exam.SubjectId
                             && q.ExamDate.Value.Date == DateTime.Today)
                    .ToList();

                DateTime examStart = exam.ExamDate.Value
                    .AddHours(exam.StartHour ?? 0)
                    .AddMinutes(exam.StartMinute ?? 0);

                DateTime examEnd = exam.ExamDate.Value
                    .AddHours(exam.EndHour ?? 23)
                    .AddMinutes(exam.EndMinute ?? 59);

                string status = "Available";
                if (DateTime.Now < examStart)
                    status = $"Not started (Starts at {examStart:hh:mm tt})";
                else if (DateTime.Now > examEnd)
                    status = "Expired";

                examData.Add(new
                {
                    exam.SubjectId,
                    exam.SubjectName,
                    exam.ExamType,
                    ExamDate = exam.ExamDate?.ToString("dd-MM-yyyy"),
                    ExamStart = examStart.ToString("yyyy-MM-ddTHH:mm:ss"),
                    ExamEnd = examEnd.ToString("yyyy-MM-ddTHH:mm:ss"),
                    Status = status,
                    Questions = questions
                });
            }

            ViewBag.Exams = examData;
            return View();
        }


        [HttpPost]
        public IActionResult SubmitExam(List<StudentAnswer> answers)
        {
            var studentId = HttpContext.Session.GetInt32("StudentId");
            if (studentId == null) return RedirectToAction("Login", "Account");

            if (answers == null || !answers.Any())
                return RedirectToAction("Index");

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

        [HttpGet]
        public IActionResult StudentRank()
        {
            var vm = new StudentRankViewModel
            {
                Categories = _context.Categories.ToList(),
                SubCategories = new List<SubCategory>(),
                Subjects = _context.Subjects.ToList(),
                StudentRanks = new List<StudentRankData>()
            };

            return View(vm);
        }


        [HttpPost]
        public IActionResult StudentRank(StudentRankViewModel model)
        {
            
            var students = _context.Student
                .Where(s => s.CategoryId == model.CategoryId && s.SubCategoryId == model.SubCategoryId)
                .ToList();

            var studentRanks = students.Select(s => new
            {
                StudentName = s.Name,
                TotalMarks = _context.StudentExamResults
                                .Where(r => r.StudentId == s.Id && r.SubjectId == model.SubjectId)
                                .Sum(r => r.CorrectAnswers), 
                StudentId = s.Id
            })
            .OrderByDescending(x => x.TotalMarks)
            .ToList();

            
            int rank = 1;
            var finalList = studentRanks.Select(x => new StudentRankData
            {
                StudentName = x.StudentName,
                TotalMarks = x.TotalMarks,
                Rank = rank++
            }).ToList();

            model.Categories = _context.Categories.ToList();
            model.SubCategories = _context.SubCategories.Where(sc => sc.CategoryId == model.CategoryId).ToList();
            model.Subjects = _context.Subjects.ToList();
            model.StudentRanks = finalList;

            return View(model);
        }

        [HttpGet]
        public JsonResult GetSubCategories(int categoryId)
        {
            var subCategories = _context.SubCategories
                .Where(sc => sc.CategoryId == categoryId)
                .Select(sc => new
                {
                    Id = sc.Id,
                    Name = sc.Name
                }).ToList();

            return Json(subCategories);
        }

    }
}
