using digital.Models;
using Digital.Models;
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

        public async Task AddAssignmentAsync(Assignment assignment)
        {
            _context.Assignment.Add(assignment);
            await _context.SaveChangesAsync();
        }

        public async Task AddAssignmentAsync(Assignment assignment, List<int> studentIds, bool assignAll)
        {
            _context.Assignment.Add(assignment);
            await _context.SaveChangesAsync();

            if (assignAll)
            {
                var allStudents = _context.Student
                    .Where(s => s.CategoryId == assignment.CategoryId && s.SubCategoryId == assignment.SubCategoryId)
                    .ToList();

                foreach (var student in allStudents)
                {
                    _context.AssignmentSubmissions.Add(new AssignmentSubmission
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
                    _context.AssignmentSubmissions.Add(new AssignmentSubmission
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
            return await _context.Assignment
                .Include(a => a.Submission)
                .ToListAsync();
        }

        public async Task<Assignment> GetAssignmentByIdAsync(int id)
        {
            return await _context.Assignment
                .Include(a => a.Submission)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task UpdateAssignmentAsync(Assignment assignment)
        {
            _context.Assignment.Update(assignment);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAssignmentAsync(int id)
        {
            var assignment = await _context.Assignment.FindAsync(id);
            if (assignment != null)
            {
                var relatedStudents = _context.AssignmentSubmissions
                                              .Where(x => x.AssignmentId == id)
                                              .ToList();

                _context.AssignmentSubmissions.RemoveRange(relatedStudents);
                _context.Assignment.Remove(assignment);

                await _context.SaveChangesAsync();
            }
        }
    }
}
