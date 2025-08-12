using digital.Models;
using System.Collections.Generic;

namespace digital.Repository
{
    public interface IExamRepository
    {
        IEnumerable<Category> GetCategories();
        IEnumerable<SubCategory> GetSubCategoriesByCategory(int categoryId);
        IEnumerable<Subject> GetSubjects();
        IEnumerable<User> GetTeachers();
        void AddExam(Exam exam);
    }
}
