using digital.Models;

namespace digital.Repositories
{
    public interface ITeacherMasterRepository
    {
        List<TeacherMaster> GetAllWithRelations();
        TeacherMaster? GetById(int id);
        void Add(TeacherMaster teacherMaster);
        void Update(TeacherMaster teacherMaster);
        void Delete(int id);

        List<Category> GetCategories();
        List<Subject> GetSubjects();
        List<SubCategory> GetSubCategoriesByCategory(int categoryId);
        List<User> GetTeachers();
        bool Exists(int categoryId, int subCategoryId, int subjectId, int teacherId);
    }
}
