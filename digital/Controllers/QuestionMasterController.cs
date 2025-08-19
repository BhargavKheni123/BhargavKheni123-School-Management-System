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
        public IActionResult Create(QuestionMaster question, List<string> answerOptions, List<bool> isCorrect)
        {
            if (ModelState.IsValid)
            {
                _context.QuestionMaster.Add(question);
                _context.SaveChanges();

                for (int i = 0; i < answerOptions.Count; i++)
                {
                    var ans = new AnswerOptions
                    {
                        QuestionId = question.Id,
                        OptionText = answerOptions[i],
                        AnswerText = answerOptions[i],
                        IsCorrect = isCorrect[i]
                    };
                    _context.AnswerOptions.Add(ans);
                }
                _context.SaveChanges();

                return RedirectToAction("Create"); 
            }

            ViewBag.CategoryList = new SelectList(_context.Categories.ToList(), "Id", "Name");
            ViewBag.SubjectList = new SelectList(_context.Subjects.ToList(), "Id", "Name");

            return View(question);
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
        public IActionResult Edit(QuestionMaster question, List<string> answerOptions, List<bool> isCorrect)
        {
            if (ModelState.IsValid)
            {
                _context.QuestionMaster.Update(question);

                var oldAnswers = _context.AnswerOptions.Where(a => a.QuestionId == question.Id).ToList();
                _context.AnswerOptions.RemoveRange(oldAnswers);

                for (int i = 0; i < answerOptions.Count; i++)
                {
                    _context.AnswerOptions.Add(new AnswerOptions
                    {
                        QuestionId = question.Id,
                        OptionText = answerOptions[i],
                        AnswerText = answerOptions[i],
                        IsCorrect = i < isCorrect.Count ? isCorrect[i] : false
                    });
                }

                _context.SaveChanges();

                return RedirectToAction("Create");
            }

            ViewBag.CategoryList = new SelectList(_context.Categories.ToList(), "Id", "Name", question.CategoryId);
            ViewBag.SubjectList = new SelectList(_context.Subjects.ToList(), "Id", "Name", question.SubjectId);

            return RedirectToAction("Create");
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
