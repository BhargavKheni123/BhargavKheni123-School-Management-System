using AutoMapper;
using digital.Interfaces;
using digital.Models;
using digital.Repositories;
using digital.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace digital.Controllers
{
    public class TimeTableController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ITimeTableRepository _timeTableRepository;
        private readonly IGenericRepository<TimeTable> _timeTableRepo;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;

        public TimeTableController(
            ApplicationDbContext context,
            ITimeTableRepository timeTableRepository,
            IGenericRepository<TimeTable> timeTableRepo,
            ICategoryRepository categoryRepository,
            IMapper mapper)
        {
            _context = context;
            _timeTableRepository = timeTableRepository;
            _timeTableRepo = timeTableRepo;
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }

        [HttpGet]
        public IActionResult TimeTableForm()
        {
            string role = HttpContext.Session.GetString("UserRole");
            string email = HttpContext.Session.GetString("UserEmail");

            var vm = new TimeTableViewModel
            {
                Role = role,
                Message = TempData["Message"] as string ?? ""
            };

            PopulateTimeTableViewModel(vm);
            return View(vm);
        }

        private void PopulateTimeTableViewModel(TimeTableViewModel vm)
        {
            vm.StdList = _categoryRepository.GetAllCategories()
                .Select(c => new SelectListItem { Value = c.Name, Text = c.Name }).ToList();

            vm.ClassList = new List<SelectListItem>();

            vm.Hours = Enumerable.Range(1, 24)
                .Select(h => new SelectListItem { Value = h.ToString(), Text = h.ToString() }).ToList();

            vm.Minutes = Enumerable.Range(0, 60)
                .Select(m => new SelectListItem { Value = m.ToString("D2"), Text = m.ToString("D2") }).ToList();

            vm.TeacherList = _context.Users
                .Where(u => u.Role == "Teacher")
                .Select(t => new SelectListItem { Value = t.Id.ToString(), Text = t.Name })
                .ToList();

            var role = HttpContext.Session.GetString("UserRole");
            if (role == "Teacher")
            {
                var teacherId = HttpContext.Session.GetInt32("TeacherId");
                vm.TimeTableList = _timeTableRepository.GetAllTimeTables()
                                   .Where(t => t.TeacherId == teacherId)
                                   .ToList();
            }
            else
            {
                vm.TimeTableList = _timeTableRepository.GetAllTimeTables();
            }
        }

        [HttpPost]
        public IActionResult TimeTableForm(TimeTableViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                TempData["FormErrors"] = string.Join(" | ", errors);

                PopulateTimeTableViewModel(model);
                return View(model);
            }

            int newStart = (model.TimeTable.StartHour * 60) + model.TimeTable.StartMinute;
            int newEnd = (model.TimeTable.EndHour * 60) + model.TimeTable.EndMinute;

            var existingRecords = _timeTableRepository.GetAllTimeTables();

            if (model.TimeTable.TeacherId.HasValue)
            {
                bool teacherConflict = existingRecords.Any(t =>
                    t.TeacherId == model.TimeTable.TeacherId &&
                    TimesOverlap(newStart, newEnd, (t.StartHour * 60) + t.StartMinute, (t.EndHour * 60) + t.EndMinute)
                );

                if (teacherConflict)
                {
                    ModelState.AddModelError("", "This teacher is already assigned to another lecture during this time.");
                    PopulateTimeTableViewModel(model);
                    return View(model);
                }
            }

            bool stdClassConflict = existingRecords.Any(t =>
                t.Std == model.TimeTable.Std &&
                t.Class == model.TimeTable.Class &&
                TimesOverlap(newStart, newEnd, (t.StartHour * 60) + t.StartMinute, (t.EndHour * 60) + t.EndMinute)
            );

            if (stdClassConflict)
            {
                ModelState.AddModelError("", "This Standard/Class already has another lecture during this time.");
                PopulateTimeTableViewModel(model);
                return View(model);
            }

            bool subjectConflict = existingRecords.Any(t =>
                t.Std == model.TimeTable.Std &&
                t.Class == model.TimeTable.Class &&
                t.Subject != model.TimeTable.Subject &&
                TimesOverlap(newStart, newEnd, (t.StartHour * 60) + t.StartMinute, (t.EndHour * 60) + t.EndMinute)
            );

            if (subjectConflict)
            {
                ModelState.AddModelError("", "This Standard/Class already has a different subject during this time.");
                PopulateTimeTableViewModel(model);
                return View(model);
            }

            try
            {
                _timeTableRepository.AddTimeTable(model.TimeTable);
                TempData["Message"] = "Time Table Saved!";
                return RedirectToAction(nameof(TimeTableForm));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Save failed: " + ex.Message);
                PopulateTimeTableViewModel(model);
                return View(model);
            }
        }

        private bool TimesOverlap(int start1, int end1, int start2, int end2)
        {
            return start1 < end2 && start2 < end1;
        }

        [HttpGet]
        public JsonResult GetSubCategoriesByStd(string stdName)
        {
            var subcategories = _context.SubCategories
                .Where(sc => sc.Category.Name == stdName)
                .Select(sc => new { name = sc.Name })
                .ToList();

            return Json(subcategories);
        }

        [HttpGet]
        public async Task<IActionResult> EditTimeTable(int id)
        {
            var record = await _timeTableRepo.GetByIdAsync(id);
            if (record == null)
                return NotFound();

            var viewModel = _mapper.Map<TimeTableViewModel>(record);

            viewModel.StdList = _context.Categories
                .Select(c => new SelectListItem { Value = c.Name, Text = c.Name }).ToList();

            viewModel.ClassList = _context.SubCategories
                .Select(sc => new SelectListItem { Value = sc.Name, Text = sc.Name }).ToList();

            viewModel.Hours = Enumerable.Range(1, 24)
                .Select(i => new SelectListItem { Value = i.ToString(), Text = i.ToString() }).ToList();

            viewModel.Minutes = Enumerable.Range(1, 60)
                .Select(i => new SelectListItem { Value = i.ToString(), Text = i.ToString() }).ToList();

            return View("UpdateTimeTable", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> EditTimeTable(TimeTableViewModel model)
        {
            if (ModelState.IsValid)
            {
                var entity = _mapper.Map<TimeTable>(model);
                await _timeTableRepo.UpdateAsync(entity);
                await _timeTableRepo.SaveAsync();

                TempData["Message"] = "Record updated successfully!";
                return RedirectToAction("TimeTableForm");
            }

            model.StdList = _context.Categories
                .Select(c => new SelectListItem { Value = c.Name, Text = c.Name }).ToList();

            model.ClassList = _context.SubCategories
                .Select(sc => new SelectListItem { Value = sc.Name, Text = sc.Name }).ToList();

            model.Hours = Enumerable.Range(1, 24)
                .Select(i => new SelectListItem { Value = i.ToString(), Text = i.ToString() }).ToList();

            model.Minutes = Enumerable.Range(1, 60)
                .Select(i => new SelectListItem { Value = i.ToString(), Text = i.ToString() }).ToList();

            return View("UpdateTimeTable", model);
        }

        public async Task<IActionResult> DeleteTimeTable(int id)
        {
            var record = await _timeTableRepo.GetByIdAsync(id);
            if (record != null)
            {
                await _timeTableRepo.DeleteAsync(record);
                await _timeTableRepo.SaveAsync();
                TempData["Message"] = "Record deleted successfully!";
            }
            return RedirectToAction("TimeTableForm");
        }
    }
}
