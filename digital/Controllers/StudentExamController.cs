using digital.Models;
using digital.Repository;
using digital.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using digital.ViewModels;

namespace digital.Controllers
{
    public class StudentExamController : Controller
    {
        private readonly IStudentExamRepository _repository;
        private readonly IStudentExamRepository _examRepo;

        public StudentExamController(IStudentExamRepository repository, IStudentExamRepository examRepo)
            
        {
            _repository = repository;
            _examRepo = examRepo;
        }

        public IActionResult Index()
        {
            var studentId = HttpContext.Session.GetInt32("StudentId");
            if (studentId == null) return RedirectToAction("Login", "Account");

            var student = _repository.GetStudentById(studentId.Value);
            if (student == null) return NotFound();

            ViewBag.StudentName = student.Name;
            ViewBag.Std = student.Category.Name;

            var exams = _repository.GetTodayExams(student.CategoryId).ToList();

            if (!exams.Any())
            {
                ViewBag.NoExamMsg = "No exam is available for today.";
                return View(new List<QuestionMaster>());
            }

            var examData = new List<dynamic>();
            foreach (var exam in exams)
            {
                var questions = _repository.GetQuestionsForExam(student.CategoryId, exam.SubjectId, exam.ExamDate);

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

                var question = _repository
                    .GetQuestionsForExam(ans.StudentId, ans.SubjectId, DateTime.Today)
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

            _repository.SaveExamResult(result, answers);

            return RedirectToAction("Result", new { resultId = result.Id });
        }

        public IActionResult Result(int resultId)
        {
            var studentId = HttpContext.Session.GetInt32("StudentId");
            if (studentId == null) return RedirectToAction("Login", "Account");

            var result = _repository.GetExamResult(resultId, studentId.Value);
            if (result == null) return NotFound();

            var answers = _repository.GetAnswersByResultId(result.Id);

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
                Categories = _repository.GetCategories().ToList(),
                SubCategories = new List<SubCategory>(),
                Subjects = _repository.GetSubjects().ToList(),
                StudentRanks = new List<StudentRankData>()
            };

            return View(vm);
        }

        [HttpPost]
        public IActionResult StudentRank(StudentRankViewModel model)
        {
            var studentRanks = _repository.GetStudentRanks(model.CategoryId, model.SubCategoryId, model.SubjectId);

            int rank = 1;
            var finalList = studentRanks.Select(x => new StudentRankData
            {
                StudentName = x.StudentName,
                TotalMarks = x.TotalMarks,
                Rank = rank++
            }).ToList();

            model.Categories = _repository.GetCategories().ToList();
            model.SubCategories = _repository.GetSubCategoriesByCategory(model.CategoryId).ToList();
            model.Subjects = _repository.GetSubjects().ToList();
            model.StudentRanks = finalList;

            return View(model);
        }

        [HttpGet]
        public JsonResult GetSubCategories(int categoryId)
        {
            var subCategories = _repository.GetSubCategoriesByCategory(categoryId)
                .Select(sc => new
                {
                    Id = sc.Id,
                    Name = sc.Name
                }).ToList();

            return Json(subCategories);
        }

        [HttpGet]
        public IActionResult StudentResultFilter()
        {
            var vm = new ExamResultFilterViewModel
            {
                Subjects = _repository.GetSubjects()
                    .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name })
                    .ToList(),
                ExamTypes = _repository.GetExamTypesByStudent(HttpContext.Session.GetInt32("StudentId") ?? 0)
                    .Select(x => new SelectListItem { Value = x, Text = x })
                    .ToList(),
                Dates = new List<SelectListItem>() 
            };

            return View(vm);
        }

        [HttpPost]
        public IActionResult StudentResultFilter(ExamResultFilterViewModel model)
        {
            var studentId = HttpContext.Session.GetInt32("StudentId");
            if (studentId == null) return RedirectToAction("Login", "Home");

            model.Subjects = _repository.GetSubjects()
                .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name })
                .ToList();

            model.ExamTypes = _repository.GetExamTypesByStudent(studentId.Value)
                .Select(x => new SelectListItem { Value = x, Text = x })
                .ToList();

            model.IsSubmitted = true;
            model.SelectedSubjectId = model.SubjectId ?? 0;
            model.SelectedExamType = model.ExamType;

            var result = _repository.GetFilteredExamResult(
                studentId.Value,
                model.SelectedSubjectId,
                model.SelectedExamType,
                string.IsNullOrEmpty(model.SelectedExamDate) ? (DateTime?)null : DateTime.Parse(model.SelectedExamDate)
            );

            if (result != null)
            {
                model.ResultViewModel = _repository.BuildExamResultViewModel(result.Id);
            }

            return View(model);
        }





    }
}
