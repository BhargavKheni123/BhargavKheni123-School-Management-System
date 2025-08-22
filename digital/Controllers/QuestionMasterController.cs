using digital.Interfaces;
using digital.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Linq;

namespace digital.Controllers
{
    public class QuestionMasterController : Controller
    {
        private readonly IQuestionMasterRepository _repository;
        private readonly ApplicationDbContext _context;

        public QuestionMasterController(IQuestionMasterRepository repository, ApplicationDbContext context)
        {
            _repository = repository;
            _context = context;
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.CategoryList = new SelectList(_context.Categories.ToList(), "Id", "Name");
            ViewBag.SubjectList = new SelectList(_context.Subjects.ToList(), "Id", "Name");

            var questions = _repository.GetAllQuestions()
                .Where(q => q.QuestionText != null)
                .ToList();

            ViewBag.QuestionList = questions;

            return View(new QuestionMaster());
        }

        [HttpPost]
        public IActionResult Create(QuestionMaster question, List<string> answerOptions, int? correctAnswerIndex)
        {
            var opts = (answerOptions ?? new List<string>())
                .Where(o => !string.IsNullOrWhiteSpace(o))
                .Take(4).ToList();

            question.Answer1 = opts.ElementAtOrDefault(0);
            question.Answer2 = opts.ElementAtOrDefault(1);
            question.Answer3 = opts.ElementAtOrDefault(2);
            question.Answer4 = opts.ElementAtOrDefault(3);

            if (correctAnswerIndex.HasValue && correctAnswerIndex.Value < answerOptions.Count)
            {
                question.RightAnswer = answerOptions[correctAnswerIndex.Value];
            }

            var answers = new List<AnswerOptions>();
            for (int i = 0; i < answerOptions.Count; i++)
            {
                if (!string.IsNullOrWhiteSpace(answerOptions[i]))
                {
                    answers.Add(new AnswerOptions
                    {
                        OptionText = answerOptions[i],
                        AnswerText = answerOptions[i],
                        IsCorrect = (correctAnswerIndex.HasValue && i == correctAnswerIndex.Value)
                    });
                }
            }

            _repository.AddQuestion(question, answers);

            return RedirectToAction("Create");
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var question = _repository.GetById(id);
            if (question == null)
                return NotFound();

            ViewBag.CategoryList = new SelectList(_context.Categories.ToList(), "Id", "Name", question.CategoryId);
            ViewBag.SubjectList = new SelectList(_context.Subjects.ToList(), "Id", "Name", question.SubjectId);

            return View(question);
        }

        [HttpPost]
        public IActionResult Edit(int id, QuestionMaster form, List<string> answerOptions, int? correctAnswerIndex)
        {
            var question = _repository.GetById(id);
            if (question == null)
                return NotFound();

            question.CategoryId = form.CategoryId;
            question.SubjectId = form.SubjectId;
            question.ExamType = form.ExamType;
            question.QuestionText = form.QuestionText;

            var answers = new List<AnswerOptions>();
            string rightAnswer = null;

            for (int i = 0; i < answerOptions.Count; i++)
            {
                if (!string.IsNullOrWhiteSpace(answerOptions[i]))
                {
                    bool isCorrect = (correctAnswerIndex.HasValue && i == correctAnswerIndex.Value);
                    if (isCorrect) rightAnswer = answerOptions[i];

                    answers.Add(new AnswerOptions
                    {
                        OptionText = answerOptions[i],
                        AnswerText = answerOptions[i],
                        IsCorrect = isCorrect
                    });
                }
            }

            question.RightAnswer = rightAnswer;

            _repository.UpdateQuestion(question, answers);

            return RedirectToAction(nameof(Create));
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            _repository.DeleteQuestion(id);
            return RedirectToAction("Create");
        }
    }
}
