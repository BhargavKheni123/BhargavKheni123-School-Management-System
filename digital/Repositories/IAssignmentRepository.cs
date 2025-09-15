using digital.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace digital.Repository
{
    public interface IAssignmentRepository
    {
        Task AddAssignmentAsync(Assignment assignment);
        Task AddAssignmentAsync(Assignment assignment, List<int> studentIds, bool assignAll);
        Task<List<Assignment>> GetAllAssignmentsAsync();
        Task<Assignment> GetAssignmentByIdAsync(int id);
        Task UpdateAssignmentAsync(Assignment assignment);
        Task DeleteAssignmentAsync(int id);
        AssignmentEvaluation GetEvaluation(int assignmentId, int teacherId);
        void SaveEvaluation(AssignmentEvaluation evaluation);
    }
}
