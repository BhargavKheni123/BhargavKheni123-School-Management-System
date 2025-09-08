using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Digital.Models;


public class AssignmentSubmissionRepository : IAssignmentSubmissionRepository
{
    private readonly ApplicationDbContext _ctx;
    public AssignmentSubmissionRepository(ApplicationDbContext ctx)
    {
        _ctx = ctx;
    }


    public async Task<AssignmentSubmission> GetByAssignmentAndStudentAsync(int assignmentId, int studentId)
    {
        return await _ctx.AssignmentSubmissions
        .FirstOrDefaultAsync(s => s.AssignmentId == assignmentId && s.StudentId == studentId);
    }


    public async Task AddAsync(AssignmentSubmission submission)
    {
        _ctx.AssignmentSubmissions.Add(submission);
        await _ctx.SaveChangesAsync();
    }


    public async Task<IEnumerable<AssignmentSubmission>> GetByAssignmentAsync(int assignmentId)
    {
        return await _ctx.AssignmentSubmissions
        .Where(s => s.AssignmentId == assignmentId)
        .ToListAsync();
    }


}