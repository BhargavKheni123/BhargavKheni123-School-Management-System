using digital.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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

        StudentExamResult GetFilteredExamResult(int studentId, int? subjectId, string examType, DateTime? examDate);

        IEnumerable<string> GetExamTypesByStudent(int studentId);
        IEnumerable<DateTime> GetExamDatesByStudent(int studentId);

        ExamResultViewModel BuildExamResultViewModel(int resultId);

        // 🔹 Extra method for exam dates by subject + type
        Task<List<DateTime>> GetExamDatesByStudentAsync(int studentId, int subjectId, string examType);
    }
}
