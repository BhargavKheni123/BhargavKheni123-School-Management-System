using digital.Models;
using digital.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace digital.Repository
{
    public class ExamRepository : IExamRepository
    {
        private readonly ApplicationDbContext _context;

        public ExamRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public IEnumerable<Category> GetCategories()
        {
            return _context.Categories.ToList();
        }

        public IEnumerable<Subject> GetSubjects()
        {
            return _context.Subjects.ToList();
        }

        public IEnumerable<User> GetTeachers()
        {
            return _context.Users.Where(u => u.Role == "Teacher").ToList();
        }

        public void AddExam(Exam exam)
        {
            _context.Exams.Add(exam);
            _context.SaveChanges();
        }

        public void UpdateExam(Exam exam)
        {
            _context.Exams.Update(exam);
            _context.SaveChanges();
        }

        public void DeleteExam(int examId)
        {
            var exam = _context.Exams.FirstOrDefault(e => e.ExamId == examId);
            if (exam != null)
            {
                _context.Exams.Remove(exam);
                _context.SaveChanges();
            }
        }

        public Exam GetExamById(int id)
        {
            return _context.Exams.FirstOrDefault(e => e.ExamId == id);
        }

        public IEnumerable<ExamListItem> GetAllExams()
        {
            return _context.Exams
                .Include(e => e.Category)
                .Include(e => e.Subject)
                .Include(e => e.AssignedTeacher)
                .Select(e => new ExamListItem
                {
                    ExamId = e.ExamId,
                    ExamTitle = e.ExamTitle,
                    ExamType = e.ExamType,
                    ClassName = e.Category != null ? e.Category.Name : "N/A",
                    SubjectName = e.Subject != null ? e.Subject.Name : "N/A",
                    TeacherName = e.AssignedTeacher != null ? e.AssignedTeacher.Name : "Unassigned",
                    AssignedTeacherId = e.AssignedTeacherId,
                    ExamDate = e.ExamDate,
                    Description = e.Description
                })
                .ToList();
        }

        public IEnumerable<ExamListItem> GetExamsByTeacherId(int teacherId)
        {
            return GetAllExams().Where(e => e.AssignedTeacherId == teacherId).ToList();
        }

        public IEnumerable<ExamListItem> GetExamsByCategoryId(int categoryId)
        {
            return GetAllExams().Where(e => e.ClassName != null && e.ClassName == _context.Categories.FirstOrDefault(c => c.Id == categoryId).Name).ToList();
        }

        public User GetTeacherByEmail(string email)
        {
            return _context.Users.FirstOrDefault(t => t.Email == email && t.Role == "Teacher");
        }

        public Student GetStudentById(int studentId)
        {
            return _context.Student.FirstOrDefault(s => s.Id == studentId);
        }
    }
}
