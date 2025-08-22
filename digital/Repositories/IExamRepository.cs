using digital.Models;
using digital.ViewModels;
using System.Collections.Generic;

namespace digital.Repository
{
    public interface IExamRepository
    {
        IEnumerable<Category> GetCategories();
        IEnumerable<Subject> GetSubjects();
        IEnumerable<User> GetTeachers();

        void AddExam(Exam exam);
        void UpdateExam(Exam exam);
        void DeleteExam(int examId);

        Exam GetExamById(int id);
        IEnumerable<ExamListItem> GetAllExams();
        IEnumerable<ExamListItem> GetExamsByTeacherId(int teacherId);
        IEnumerable<ExamListItem> GetExamsByCategoryId(int categoryId);

        User GetTeacherByEmail(string email);
        Student GetStudentById(int studentId);
    }
}
