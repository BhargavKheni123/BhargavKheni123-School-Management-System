using AutoMapper;
using ClosedXML.Excel;
using digital.Interfaces;
using digital.Models;
using digital.Repositories;
using digital.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.IO;



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

        public IActionResult ExportTimeTableToExcel()
        {
            var timetables = _context.TimeTables
                .Include(t => t.Teacher)
                .ToList();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("TimeTables");

            
            worksheet.Cell(1, 1).Value = "Std";
            worksheet.Cell(1, 2).Value = "Class";
            worksheet.Cell(1, 3).Value = "Subject";
            worksheet.Cell(1, 4).Value = "Start Time";
            worksheet.Cell(1, 5).Value = "End Time";
            worksheet.Cell(1, 6).Value = "Teacher";

          
            for (int i = 0; i < timetables.Count; i++)
            {
                var t = timetables[i];
                worksheet.Cell(i + 2, 1).Value = t.Std;
                worksheet.Cell(i + 2, 2).Value = t.Class;
                worksheet.Cell(i + 2, 3).Value = t.Subject;
                worksheet.Cell(i + 2, 4).Value = $"{t.StartHour:D2}:{t.StartMinute:D2}";
                worksheet.Cell(i + 2, 5).Value = $"{t.EndHour:D2}:{t.EndMinute:D2}";
                worksheet.Cell(i + 2, 6).Value = t.Teacher?.Name ?? "-";
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var content = stream.ToArray();
            return File(content,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "TimeTables.xlsx");
        }

        public IActionResult ExportTimeTableToPdf()
        {
            var timetables = _context.TimeTables
                .Include(t => t.Teacher)
                .ToList();

            var pdf = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.Header().Text("TimeTable List").SemiBold().FontSize(20).AlignCenter();
                    page.Content().Table(table =>
                    {
                       
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                        });

                      
                        table.Header(header =>
                        {
                            header.Cell().Text("Std").Bold();
                            header.Cell().Text("Class").Bold();
                            header.Cell().Text("Subject").Bold();
                            header.Cell().Text("Start Time").Bold();
                            header.Cell().Text("End Time").Bold();
                            header.Cell().Text("Teacher").Bold();
                        });

                      
                        foreach (var t in timetables)
                        {
                            table.Cell().Text(t.Std);
                            table.Cell().Text(t.Class);
                            table.Cell().Text(t.Subject);
                            table.Cell().Text($"{t.StartHour:D2}:{t.StartMinute:D2}");
                            table.Cell().Text($"{t.EndHour:D2}:{t.EndMinute:D2}");
                            table.Cell().Text(t.Teacher?.Name ?? "-");
                        }
                    });
                });
            });

            var pdfBytes = pdf.GeneratePdf();
            return File(pdfBytes, "application/pdf", "TimeTables.pdf");
        }

    }
}
