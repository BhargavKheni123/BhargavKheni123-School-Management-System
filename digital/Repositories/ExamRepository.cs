using digital.Interfaces;
using digital.Models;
using digital.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using digital.Models;

namespace digital.Repositories
{
    public class ExamRepository : IExamRepository
    {
        private readonly ApplicationDbContext _context;

        public ExamRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Exam>> GetAllAsync()
        {
            return await _context.Exams
                .Include(e => e.Category)
                .Include(e => e.SubCategory)
                .Include(e => e.ExamTeachers)
                    .ThenInclude(et => et.Teacher)
                .ToListAsync();
        }

        public async Task<Exam> GetByIdAsync(int id)
        {
            return await _context.Exams
                .Include(e => e.Category)
                .Include(e => e.SubCategory)
                .Include(e => e.ExamTeachers)
                    .ThenInclude(et => et.Teacher)
                .FirstOrDefaultAsync(e => e.ExamId == id);
        }

        public async Task AddAsync(ExamViewModel vm, int createdBy)
        {
            
        }

        public async Task UpdateAsync(ExamViewModel vm)
        {
            
        }

        public async Task DeleteAsync(int id)
        {
            var exam = await _context.Exams.FindAsync(id);
            if (exam != null)
            {
                _context.Exams.Remove(exam);
                await _context.SaveChangesAsync();
            }
        }
    }
}
