using digital.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace digital.Controllers
{
    public class QuestionMasterController : Controller
    {
        private readonly ApplicationDbContext _context;

        public QuestionMasterController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.CategoryList = new SelectList(_context.Categories.ToList(), "Id", "Name");
            ViewBag.SubjectList = new SelectList(_context.Subjects.ToList(), "Id", "Name");

            var questions = _context.QuestionMaster
    .Include(q => q.Category)
    .Include(q => q.Subject)
    .Include(q => q.AnswerOptions)
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

                _context.QuestionMaster.Add(question);
                _context.SaveChanges();

                for (int i = 0; i < answerOptions.Count; i++)
                {
                    if (!string.IsNullOrWhiteSpace(answerOptions[i]))
                    {
                        var ans = new AnswerOptions
                        {
                            QuestionId = question.Id,
                            OptionText = answerOptions[i],
                            AnswerText = answerOptions[i],
                            IsCorrect = (correctAnswerIndex.HasValue && i == correctAnswerIndex.Value)
                        };
                        _context.AnswerOptions.Add(ans);
                    }
                }


            ViewBag.CategoryList = new SelectList(_context.Categories.ToList(), "Id", "Name");
            ViewBag.SubjectList = new SelectList(_context.Subjects.ToList(), "Id", "Name");
            _context.SaveChanges();
                return RedirectToAction("Create");
            

        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var question = _context.QuestionMaster
                .Include(q => q.AnswerOptions)
                .FirstOrDefault(q => q.Id == id);

            if (question == null)
                return NotFound();

            ViewBag.CategoryList = new SelectList(_context.Categories.ToList(), "Id", "Name", question.CategoryId);
            ViewBag.SubjectList = new SelectList(_context.Subjects.ToList(), "Id", "Name", question.SubjectId);

            return View(question);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, QuestionMaster form, List<string> answerOptions, int? correctAnswerIndex)
        {
            var question = _context.QuestionMaster
                .Include(q => q.AnswerOptions)
                .FirstOrDefault(q => q.Id == id);

            if (question == null)
                return NotFound();

            question.CategoryId = form.CategoryId;
            question.SubjectId = form.SubjectId;
            question.ExamType = form.ExamType;
            question.QuestionText = form.QuestionText;

            _context.AnswerOptions.RemoveRange(question.AnswerOptions);

            string rightAnswer = null;
            for (int i = 0; i < answerOptions.Count; i++)
            {
                if (!string.IsNullOrWhiteSpace(answerOptions[i]))
                {
                    bool isCorrect = (correctAnswerIndex.HasValue && i == correctAnswerIndex.Value);
                    if (isCorrect) rightAnswer = answerOptions[i];

                    _context.AnswerOptions.Add(new AnswerOptions
                    {
                        QuestionId = question.Id,
                        OptionText = answerOptions[i],
                        AnswerText = answerOptions[i],
                        IsCorrect = isCorrect
                    });
                }
            }

            question.RightAnswer = rightAnswer;
            _context.SaveChanges();

            return RedirectToAction(nameof(Create));
        }



        [HttpGet]
        public IActionResult Delete(int id)
        {
            var question = _context.QuestionMaster
                .Include(q => q.AnswerOptions)
                .FirstOrDefault(q => q.Id == id);

            if (question != null)
            {
                _context.AnswerOptions.RemoveRange(question.AnswerOptions);
                _context.QuestionMaster.Remove(question);
                _context.SaveChanges();
            }

            return RedirectToAction("Create");
        }

    }
}
