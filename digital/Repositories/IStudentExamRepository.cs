using digital.Models;
using System.Collections.Generic;

namespace digital.Repository
{
    public interface IStudentExamRepository
    {
        Student GetStudentById(int studentId);
        IEnumerable<dynamic> GetTodayExams(int categoryId);
        List<QuestionMaster> GetQuestionsForExam(int categoryId, int subjectId, DateTime examDate);

        void SaveExamResult(StudentExamResult result, List<StudentAnswer> answers);

        StudentExamResult GetExamResult(int resultId, int studentId);
        List<StudentAnswer> GetAnswersByResultId(int resultId);

        IEnumerable<Category> GetCategories();
        IEnumerable<SubCategory> GetSubCategoriesByCategory(int categoryId);
        IEnumerable<Subject> GetSubjects();

        IEnumerable<dynamic> GetStudentRanks(int categoryId, int subCategoryId, int subjectId);
    }
}
