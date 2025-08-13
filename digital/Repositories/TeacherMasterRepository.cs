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
                .ToList();
        }
        
    }

}
