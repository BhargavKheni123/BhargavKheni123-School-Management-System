using digital.Models;
using Microsoft.EntityFrameworkCore;

namespace digital.Repositories
{
    public class TeacherMasterRepository : ITeacherMasterRepository
    {
        private readonly ApplicationDbContext _context;

        public TeacherMasterRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<TeacherMaster> GetAllWithRelations()
        {
            return _context.TeacherMaster
                .Include(tm => tm.Category)
                .Include(tm => tm.SubCategory)
                .Include(tm => tm.Subject)
                .Include(tm => tm.Teacher)
                .OrderByDescending(tm => tm.CreatedDate)
                .ToList();
        }

        public TeacherMaster? GetById(int id)
        {
            return _context.TeacherMaster
                .Include(tm => tm.Category)
                .Include(tm => tm.SubCategory)
                .Include(tm => tm.Subject)
                .Include(tm => tm.Teacher)
                .FirstOrDefault(tm => tm.Id == id);
        }

        public void Add(TeacherMaster teacherMaster)
        {
            _context.TeacherMaster.Add(teacherMaster);
            _context.SaveChanges();
        }

        public void Update(TeacherMaster teacherMaster)
        {
            _context.TeacherMaster.Update(teacherMaster);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var entity = _context.TeacherMaster.Find(id);
            if (entity != null)
            {
                _context.TeacherMaster.Remove(entity);
                _context.SaveChanges();
            }
        }

        public List<Category> GetCategories() => _context.Categories.ToList();
        public List<Subject> GetSubjects() => _context.Subjects.ToList();
        public List<SubCategory> GetSubCategoriesByCategory(int categoryId) =>
            _context.SubCategories.Where(sc => sc.CategoryId == categoryId).ToList();
        public List<User> GetTeachers() => _context.Users.Where(u => u.Role == "Teacher").ToList();

        public bool Exists(int categoryId, int subCategoryId, int subjectId, int teacherId)
        {
            return _context.TeacherMaster.Any(x =>
                x.CategoryId == categoryId &&
                x.SubCategoryId == subCategoryId &&
                x.SubjectId == subjectId &&
                x.TeacherId == teacherId);
        }
    }
}
