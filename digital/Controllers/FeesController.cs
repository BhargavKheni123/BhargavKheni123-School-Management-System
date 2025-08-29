using digital.Interfaces;
using digital.Models;
using digital.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace digital.Controllers
{
    public class FeesController : Controller
    {
        private readonly IFeesRepository _feesRepository;

        public FeesController(IFeesRepository feesRepository)
        {
            _feesRepository = feesRepository;
        }

        
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            ViewBag.Categories = await _feesRepository.GetCategoriesAsync();
            ViewBag.Years = await _feesRepository.GetYearsAsync();
            return View();
        }

        
        [HttpPost]
        public async Task<IActionResult> GetReport(int year, int categoryId, decimal fees)
        {
            var report = await _feesRepository.GetFeesReportAsync(year, categoryId);

            
            var updatedReport = report.Select(r =>
                (r.student, totalFees: fees, r.paidFees, r.balance = fees - r.paidFees)).ToList();

            ViewBag.Year = year;
            ViewBag.Category = await _feesRepository.GetCategoryNameAsync(categoryId);

            return PartialView("_FeesReportPartial", updatedReport);
        }

        
        [HttpGet]
        public async Task<IActionResult> CreatePayment(int studentId, int year)
        {
            ViewBag.Student = await _feesRepository.GetStudentByIdAsync(studentId);
            ViewBag.Year = year;
            return View();
        }

        
        [HttpPost]
        public async Task<IActionResult> CreatePayment(StudentFees model)
        {
            if (ModelState.IsValid)
            {
                await _feesRepository.AddStudentFeeAsync(model);
                return RedirectToAction("Index");
            }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> GetCategoryFees(int categoryId)
        {
            var category = await _feesRepository.GetCategoryByIdAsync(categoryId);
            if (category != null)
            {
                return Json(new { success = true, fees = category.Fees });
            }
            return Json(new { success = false, fees = 0 });
        }

        [HttpGet]
        public async Task<IActionResult> MyFees()
        {
            var studentId = HttpContext.Session.GetInt32("StudentId"); 
            if (studentId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var student = await _feesRepository.GetStudentByIdAsync(studentId.Value);
            if (student == null)
                return NotFound();

            int currentYear = DateTime.Now.Year;

            
            var category = await _feesRepository.GetCategoryByIdAsync(student.CategoryId);
            decimal totalFees = category?.Fees ?? 0;

            
            decimal paidFees = await _feesRepository.GetStudentPaidAmountAsync(studentId.Value, currentYear);

            var vm = new StudentFeeViewModel
            {
                StudentName = student.Name,
                Year = currentYear,
                TotalFees = totalFees,
                PaidFees = paidFees
            };

            return View(vm);
        }


    }
}
