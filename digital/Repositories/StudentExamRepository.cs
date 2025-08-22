using digital.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace digital.Repository
{
    public class StudentExamRepository : IStudentExamRepository
    {
        private readonly ApplicationDbContext _context;

        public StudentExamRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public Student GetStudentById(int studentId)
        {
            return _context.Student
                .Include(s => s.Category)
                .FirstOrDefault(s => s.Id == studentId);
        }

        public IEnumerable<dynamic> GetTodayExams(int categoryId)
        {
            var exams = (from q in _context.QuestionMaster
                         join s in _context.Subjects on q.SubjectId equals s.Id
                         where q.CategoryId == categoryId
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

            return exams;
        }

        public List<QuestionMaster> GetQuestionsForExam(int categoryId, int subjectId, DateTime examDate)
        {
            return _context.QuestionMaster
                .Include(q => q.AnswerOptions)
                .Where(q => q.CategoryId == categoryId
                         && q.SubjectId == subjectId
                         && q.ExamDate.Value.Date == examDate.Date)
                .ToList();
        }

        public void SaveExamResult(StudentExamResult result, List<StudentAnswer> answers)
        {
            _context.StudentExamResults.Add(result);
            _context.SaveChanges();

            foreach (var ans in answers)
            {
                ans.ResultId = result.Id;
            }

            _context.StudentAnswers.AddRange(answers);
            _context.SaveChanges();
        }

        public StudentExamResult GetExamResult(int resultId, int studentId)
        {
            return _context.StudentExamResults
                .Include(r => r.Subject)
                .Include(r => r.Student)
                .ThenInclude(s => s.Category)
                .FirstOrDefault(r => r.Id == resultId && r.StudentId == studentId);
        }

        public List<StudentAnswer> GetAnswersByResultId(int resultId)
        {
            return _context.StudentAnswers
                .Include(a => a.Question)
                .ThenInclude(q => q.AnswerOptions)
                .Where(a => a.ResultId == resultId)
                .ToList();
        }

        public IEnumerable<Category> GetCategories()
        {
            return _context.Categories.ToList();
        }

        public IEnumerable<SubCategory> GetSubCategoriesByCategory(int categoryId)
        {
            return _context.SubCategories
                .Where(sc => sc.CategoryId == categoryId)
                .ToList();
        }

        public IEnumerable<Subject> GetSubjects()
        {
            return _context.Subjects.ToList();
        }

        public IEnumerable<dynamic> GetStudentRanks(int categoryId, int subCategoryId, int subjectId)
        {
            var students = _context.Student
                .Where(s => s.CategoryId == categoryId && s.SubCategoryId == subCategoryId)
                .ToList();

            var studentRanks = students.Select(s => new
            {
                StudentName = s.Name,
                TotalMarks = _context.StudentExamResults
                                .Where(r => r.StudentId == s.Id && r.SubjectId == subjectId)
                                .Sum(r => r.CorrectAnswers),
                StudentId = s.Id
            })
            .OrderByDescending(x => x.TotalMarks)
            .ToList();

            return studentRanks;
        }
    }
}
