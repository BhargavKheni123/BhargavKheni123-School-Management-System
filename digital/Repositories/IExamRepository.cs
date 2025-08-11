using digital.Models;
using digital.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;
using digital.Models;

namespace digital.Interfaces
{
    public interface IExamRepository
    {
        Task<List<Exam>> GetAllAsync();
        Task<Exam> GetByIdAsync(int id);
        Task AddAsync(ExamViewModel vm, int createdBy);
        Task UpdateAsync(ExamViewModel vm);
        Task DeleteAsync(int id);
    }
}
