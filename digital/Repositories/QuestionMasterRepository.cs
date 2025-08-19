using digital.Interfaces;
using digital.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace digital.Repositories
{
    public class QuestionMasterRepository : IQuestionMasterRepository
    {
        private readonly ApplicationDbContext _context;

        public QuestionMasterRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public IEnumerable<QuestionMaster> GetAllQuestions()
        {
            return _context.QuestionMaster
                .Include(q => q.Category)
                .Include(q => q.Subject)
                .Include(q => q.AnswerOptions) 
                .ToList();
        }

        public QuestionMaster GetById(int id)
        {
            return _context.QuestionMaster
                .Include(q => q.AnswerOptions)
                .FirstOrDefault(q => q.Id == id);
        }

        public void AddQuestion(QuestionMaster question, List<AnswerOptions> answers)
        {
            // Add Question first
            _context.QuestionMaster.Add(question);
            _context.SaveChanges();

            // Add related Answers
            foreach (var ans in answers)
            {
                ans.QuestionId = question.Id;
                _context.AnswerOptions.Add(ans);
            }

            _context.SaveChanges();
        }

        public void UpdateQuestion(QuestionMaster question, List<AnswerOptions> answers)
        {
            _context.QuestionMaster.Update(question);

            // Remove old answers
            var existingAnswers = _context.AnswerOptions
                .Where(a => a.QuestionId == question.Id)
                .ToList();

            _context.AnswerOptions.RemoveRange(existingAnswers);

            // Add new answers
            foreach (var ans in answers)
            {
                ans.QuestionId = question.Id;
                _context.AnswerOptions.Add(ans);
            }

            _context.SaveChanges();
        }

        public void DeleteQuestion(int id)
        {
            var question = _context.QuestionMaster
                .Include(q => q.AnswerOptions)
                .FirstOrDefault(q => q.Id == id);

            if (question != null)
            {
                // Remove answers first
                if (question.AnswerOptions != null && question.AnswerOptions.Any())
                {
                    _context.AnswerOptions.RemoveRange(question.AnswerOptions);
                }

                _context.QuestionMaster.Remove(question);
                _context.SaveChanges();
            }
        }
    }
}
