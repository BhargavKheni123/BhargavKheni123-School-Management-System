using digital.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace digital.Repository
{
    public interface IAssignmentRepository
    {
        Task AddAssignmentAsync(Assignment assignment, List<int> studentIds, bool assignAll);
        Task<List<Assignment>> GetAllAssignmentsAsync();
    }
}
