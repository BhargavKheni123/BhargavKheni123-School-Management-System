using digital.Models;
using System.Collections.Generic;

namespace digital.Repository
{
    public interface IExamRepository
    {
        IEnumerable<Category> GetCategories();

        IEnumerable<Subject> GetSubjects();
        IEnumerable<User> GetTeachers();
        void AddExam(Exam exam);

    }
}

