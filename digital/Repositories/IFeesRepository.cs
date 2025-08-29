using digital.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace digital.Interfaces
{
    public interface IFeesRepository
    {
      
        Task<List<StandardFees>> GetStandardFeesAsync();
        Task<StandardFees?> GetStandardFeeByYearAndCategoryAsync(int year, int categoryId);
        Task AddStandardFeeAsync(StandardFees fee);

        Task<List<StudentFees>> GetStudentFeesAsync(int studentId, int year);
        Task<decimal> GetStudentPaidAmountAsync(int studentId, int year);
        Task AddStudentFeeAsync(StudentFees fee);

        Task<List<(Student student, decimal totalFees, decimal paidFees, decimal balance)>>
            GetFeesReportAsync(int year, int categoryId);

        Task<List<SelectListItem>> GetCategoriesAsync();
        Task<List<SelectListItem>> GetYearsAsync();
        Task<Category?> GetCategoryByIdAsync(int categoryId);



        Task<Student?> GetStudentByIdAsync(int studentId);
        Task<string?> GetCategoryNameAsync(int categoryId);
    }
}
