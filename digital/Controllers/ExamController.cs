using digital.Models;
using digital.ViewModels;
using digital.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace digital.Controllers
{
    public class ExamController : Controller
    {
        private readonly IExamRepository _repository;

        public ExamController(IExamRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public IActionResult CreateExam()
        {
            var loggedInUserEmail = User.Identity?.Name ?? HttpContext.Session.GetString("UserEmail");
            var loggedInTeacher = _repository.GetTeacherByEmail(loggedInUserEmail);

            var model = new ExamViewModel
            {
                Categories = _repository.GetCategories()
                    .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name }).ToList(),
                Subjects = _repository.GetSubjects()
                    .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name }).ToList(),
                Teachers = _repository.GetTeachers()
                    .Select(t => new SelectListItem { Value = t.Id.ToString(), Text = t.Name }).ToList(),
                ExamList = (User.IsInRole("Teacher") && loggedInTeacher != null)
                    ? _repository.GetExamsByTeacherId(loggedInTeacher.Id).ToList()
                    : _repository.GetAllExams().ToList()
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult CreateExam(ExamViewModel model)
        {
            model.Categories = _repository.GetCategories()
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name }).ToList();
            model.Subjects = _repository.GetSubjects()
                .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name }).ToList();
            model.Teachers = _repository.GetTeachers()
                .Select(t => new SelectListItem { Value = t.Id.ToString(), Text = t.Name }).ToList();

            var exam = new Exam
            {
                ExamTitle = model.ExamTitle,
                Description = model.Description,
                ExamType = model.ExamType,
                CategoryId = model.CategoryId.GetValueOrDefault(),
                SubjectId = model.SubjectId.GetValueOrDefault(),
                AssignedTeacherId = model.AssignedTeacherId,
                ExamDate = model.ExamDate,
                CreatedBy = 1,
                CreatedDate = DateTime.Now
            };

            _repository.AddExam(exam);

            TempData["Success"] = "Exam created successfully!";
            return RedirectToAction("CreateExam");
        }

        [HttpGet]
        public IActionResult EditExam(int id)
        {
            var exam = _repository.GetExamById(id);
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

                Categories = _repository.GetCategories()
                    .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name }).ToList(),
                Subjects = _repository.GetSubjects()
                    .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name }).ToList(),
                Teachers = _repository.GetTeachers()
                    .Select(t => new SelectListItem { Value = t.Id.ToString(), Text = t.Name }).ToList()
            };

            return View("EditExam", model);
        }

        [HttpPost]
        public IActionResult EditExam(ExamViewModel model)
        {
            var exam = _repository.GetExamById(model.ExamId);
            if (exam == null)
                return RedirectToAction("CreateExam");

            exam.ExamTitle = model.ExamTitle;
            exam.Description = model.Description;
            exam.ExamType = model.ExamType;
            exam.CategoryId = model.CategoryId.GetValueOrDefault();
            exam.SubjectId = model.SubjectId.GetValueOrDefault();
            exam.AssignedTeacherId = model.AssignedTeacherId;
            exam.ExamDate = model.ExamDate;

            _repository.UpdateExam(exam);

            TempData["Success"] = "Exam updated successfully!";
            return RedirectToAction("CreateExam");
        }

        [HttpGet]
        public IActionResult DeleteExam(int id)
        {
            _repository.DeleteExam(id);
            TempData["Success"] = "Exam deleted successfully!";
            return RedirectToAction("CreateExam");
        }

        [HttpGet]
        public IActionResult StudentExamList()
        {
            var studentId = HttpContext.Session.GetInt32("StudentId");
            if (studentId == null)
                return RedirectToAction("StudentDetails", "Student");

            var student = _repository.GetStudentById(studentId.Value);
            if (student == null)
                return NotFound();

            var exams = _repository.GetExamsByCategoryId(student.CategoryId).ToList();

            var model = new ExamViewModel { ExamList = exams };
            return View(model);
        }

        [HttpGet]
        public IActionResult TeacherExamList()
        {
            var loggedInUserEmail = User.Identity?.Name ?? HttpContext.Session.GetString("UserEmail");
            var teacher = _repository.GetTeacherByEmail(loggedInUserEmail);

            if (teacher == null)
            {
                ViewBag.Message = "No exams found for this teacher.";
                return View(new ExamViewModel { ExamList = new List<ExamListItem>() });
            }

            var exams = _repository.GetExamsByTeacherId(teacher.Id).ToList();

            var model = new ExamViewModel { ExamList = exams };
            return View(model);
        }
    }
}
