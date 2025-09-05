using digital.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace digital.Repository
{
    public class AssignmentRepository : IAssignmentRepository
    {
        private readonly ApplicationDbContext _context;

        public AssignmentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAssignmentAsync(Assignment assignment, List<int> studentIds, bool assignAll)
        {
            _context.Assignments.Add(assignment);
            await _context.SaveChangesAsync();

            if (assignAll)
            {
                var allStudents = _context.Student
                    .Where(s => s.CategoryId == assignment.CategoryId && s.SubCategoryId == assignment.SubCategoryId)
                    .ToList();

                foreach (var student in allStudents)
                {
                    _context.AssignmentStudents.Add(new AssignmentStudent
                    {
                        AssignmentId = assignment.Id,
                        StudentId = student.Id
                    });
                }
            }
            else
            {
                foreach (var studentId in studentIds)
                {
                    _context.AssignmentStudents.Add(new AssignmentStudent
                    {
                        AssignmentId = assignment.Id,
                        StudentId = studentId
                    });
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task<List<Assignment>> GetAllAssignmentsAsync()
        {
            return await _context.Assignments
                .Include(a => a.AssignmentStudents)
                .ToListAsync();
        }
    }
}
