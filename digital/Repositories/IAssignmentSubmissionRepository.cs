using System.Collections.Generic;
using System.Threading.Tasks;
using Digital.Models;


public interface IAssignmentSubmissionRepository
{
    Task<AssignmentSubmission> GetByAssignmentAndStudentAsync(int assignmentId, int studentId);
    Task AddAsync(AssignmentSubmission submission);
    Task<IEnumerable<AssignmentSubmission>> GetByAssignmentAsync(int assignmentId);
}