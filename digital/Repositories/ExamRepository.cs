using digital.Models;
using System.Collections.Generic;
using System.Linq;

namespace digital.Repository
{
    public class ExamRepository : IExamRepository
    {
        private readonly   ApplicationDbContext _context;

        public ExamRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public IEnumerable<Category> GetCategories()
        {
            return _context.Categories.ToList();
        }

        public IEnumerable<SubCategory> GetSubCategoriesByCategory(int categoryId)
        {
            return _context.SubCategories
                           .Where(s => s.CategoryId == categoryId)
                           .ToList();
        }

        public IEnumerable<Subject> GetSubjects()
        {
            return _context.Subjects.ToList();
        }

        public IEnumerable<User> GetTeachers()
        {
            return _context.Users
                           .Where(u => u.Role == "Teacher")
                           .ToList();
        }

        public void AddExam(Exam exam)
        {
            _context.Exams.Add(exam);
            _context.SaveChanges();
        }
    }
}
