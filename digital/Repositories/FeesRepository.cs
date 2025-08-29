using digital.Interfaces;
using digital.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace digital.Repositories
{
    public class FeesRepository : IFeesRepository
    {
        private readonly ApplicationDbContext _context;

        public FeesRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<StandardFees>> GetStandardFeesAsync()
        {
            return await _context.StandardFees.ToListAsync();
        }

        public async Task<StandardFees?> GetStandardFeeByYearAndCategoryAsync(int year, int categoryId)
        {
            return await _context.StandardFees
                .FirstOrDefaultAsync(f => f.Year == year && f.CategoryId == categoryId);
        }

        public async Task AddStandardFeeAsync(StandardFees fee)
        {
            _context.StandardFees.Add(fee);
            await _context.SaveChangesAsync();
        }

      
        public async Task<List<StudentFees>> GetStudentFeesAsync(int studentId, int year)
        {
            return await _context.StudentFees
                .Where(f => f.StudentId == studentId && f.Year == year)
                .ToListAsync();
        }

        public async Task<decimal> GetStudentPaidAmountAsync(int studentId, int year)
        {
            return await _context.StudentFees
                .Where(f => f.StudentId == studentId && f.Year == year)
                .SumAsync(f => f.PaidAmount);
        }

        public async Task AddStudentFeeAsync(StudentFees fee)
        {
            _context.StudentFees.Add(fee);
            await _context.SaveChangesAsync();
        }

        
        public async Task<List<(Student student, decimal totalFees, decimal paidFees, decimal balance)>>
            GetFeesReportAsync(int year, int categoryId)
        {
            var students = await _context.Student
                .Where(s => s.CategoryId == categoryId)
                .ToListAsync();

            var standardFee = await GetStandardFeeByYearAndCategoryAsync(year, categoryId);
            decimal totalFees = standardFee?.TotalFees ?? 0;

            var report = new List<(Student, decimal, decimal, decimal)>();

            foreach (var s in students)
            {
                decimal paid = await GetStudentPaidAmountAsync(s.Id, year);
                decimal balance = totalFees - paid;
                report.Add((s, totalFees, paid, balance));
            }

            return report;
        }

     
        public async Task<List<SelectListItem>> GetCategoriesAsync()
        {
            return await _context.Categories
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                }).ToListAsync();
        }

        public async Task<List<SelectListItem>> GetYearsAsync()
        {
            int currentYear = DateTime.Now.Year;
            return Enumerable.Range(2021, currentYear - 2020)
                .Select(y => new SelectListItem
                {
                    Value = y.ToString(),
                    Text = y.ToString()
                }).ToList();
        }

       
        public async Task<Student?> GetStudentByIdAsync(int studentId)
        {
            return await _context.Student.FirstOrDefaultAsync(s => s.Id == studentId);
        }

        public async Task<Category?> GetCategoryByIdAsync(int categoryId)
        {
            return await _context.Categories.FirstOrDefaultAsync(c => c.Id == categoryId);
        }


        public async Task<string?> GetCategoryNameAsync(int categoryId)
        {
            return await _context.Categories
                .Where(c => c.Id == categoryId)
                .Select(c => c.Name)
                .FirstOrDefaultAsync();
        }
    }
}
