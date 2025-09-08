using digital.Models;
using digital.Repository;
using digital.ViewModels;
using DocumentFormat.OpenXml.Packaging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace digital.Controllers
{
    public class AssignmentController : Controller
    {
        private readonly IAssignmentRepository _repository;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AssignmentController(IAssignmentRepository repository, ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _repository = repository;
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        public IActionResult Create()
        {
            var vm = new AssignmentViewModel
            {
                Categories = _context.Categories.Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name }).ToList(),
                Subjects = _context.Subjects.Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name }).ToList(),
                AssignmentList = _context.Assignment.ToList()
            };
            return View(vm);
        }
        [HttpGet]
        public JsonResult GetSubCategories(int categoryId)
        {
            var subcategories = _context.SubCategories
                .Where(sc => sc.CategoryId == categoryId)
                .Select(sc => new { id = sc.Id, name = sc.Name })
                .ToList();

            return Json(subcategories);
        }

        [HttpGet]
        public JsonResult GetStudents(int categoryId, int subCategoryId)
        {
            var students = _context.Student
                .Where(s => s.CategoryId == categoryId && s.SubCategoryId == subCategoryId)
                .Select(s => new { id = s.Id, name = s.Name })
                .ToList();

            return Json(students);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AssignmentViewModel vm)
        {
            string wwwRootPath = _webHostEnvironment.WebRootPath;
            string folderPath = Path.Combine(wwwRootPath, "uploads");

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            string uniqueFileName = Guid.NewGuid().ToString();
            string filePath = "";

            if (vm.FileType == "PDF")
            {
                filePath = Path.Combine(folderPath, uniqueFileName + ".pdf");

                var doc = QuestPDF.Fluent.Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Content().Padding(20).Column(col =>
                        {
                            col.Item().Text($"Title: {vm.Title}");
                            col.Item().Text($"Description: {vm.Description}");
                            col.Item().Text($"Standard: {vm.CategoryId}");
                            col.Item().Text($"Class: {vm.SubCategoryId}");
                            col.Item().Text($"Subject: {vm.SubjectId}");
                            col.Item().Text($"Marks: {vm.Marks}");
                            col.Item().Text($"Deadline: {vm.SubmissionDeadline:dd-MM-yyyy}");
                        });
                    });
                });

                doc.GeneratePdf(filePath);
            }
            else if (vm.FileType == "Word")
            {
                filePath = Path.Combine(folderPath, uniqueFileName + ".docx");

                using (var doc = WordprocessingDocument.Create(filePath, DocumentFormat.OpenXml.WordprocessingDocumentType.Document))
                {
                    var mainPart = doc.AddMainDocumentPart();
                    mainPart.Document = new DocumentFormat.OpenXml.Wordprocessing.Document();
                    var body = new DocumentFormat.OpenXml.Wordprocessing.Body();

                    body.Append(new DocumentFormat.OpenXml.Wordprocessing.Paragraph(
                        new DocumentFormat.OpenXml.Wordprocessing.Run(
                            new DocumentFormat.OpenXml.Wordprocessing.Text($"Title: {vm.Title}")
                        )));

                    body.Append(new DocumentFormat.OpenXml.Wordprocessing.Paragraph(
                        new DocumentFormat.OpenXml.Wordprocessing.Run(
                            new DocumentFormat.OpenXml.Wordprocessing.Text($"Description: {vm.Description}")
                        )));

                    mainPart.Document.Append(body);
                }
            }

            var assignment = new Assignment
            {
                Title = vm.Title,
                Description = vm.Description,
                CategoryId = vm.CategoryId,
                SubCategoryId = vm.SubCategoryId,
                SubjectId = vm.SubjectId,
                Marks = vm.Marks,
                SubmissionDeadline = vm.SubmissionDeadline,
                FilePath = filePath.Replace(wwwRootPath + "\\", ""),
                FileType = vm.FileType,
                CreatedDate = DateTime.Now
            };

                _context.Assignment.Add(assignment);
                await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Create));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var assignment = await _repository.GetAssignmentByIdAsync(id);
            if (assignment == null) return NotFound();

            var vm = new AssignmentViewModel
            {
                Id = assignment.Id,
                Title = assignment.Title,
                Description = assignment.Description,
                CategoryId = assignment.CategoryId,
                SubCategoryId = assignment.SubCategoryId,
                SubjectId = assignment.SubjectId,
                Marks = assignment.Marks,   
                SubmissionDeadline = assignment.SubmissionDeadline,
                FilePath = assignment.FilePath,

                Categories = _context.Categories.Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name }).ToList(),
                Subjects = _context.Subjects.Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name }).ToList()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AssignmentViewModel vm)
        {
            var assignment = await _repository.GetAssignmentByIdAsync(vm.Id);
            if (assignment == null) return NotFound();

            assignment.Title = vm.Title;
            assignment.Description = vm.Description;
            assignment.CategoryId = vm.CategoryId;
            assignment.SubCategoryId = vm.SubCategoryId;
            assignment.SubjectId = vm.SubjectId;
            assignment.Marks = vm.Marks;
            assignment.SubmissionDeadline = vm.SubmissionDeadline;

            await _repository.UpdateAssignmentAsync(assignment);

            return RedirectToAction(nameof(Create));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            await _repository.DeleteAssignmentAsync(id);
            return RedirectToAction(nameof(Create));
        }

        public async Task<IActionResult> Index()
        {
            var assignments = await _repository.GetAllAssignmentsAsync();
            return View(assignments);
        }

        public async Task<IActionResult> DownloadStudentSubmission(int submissionId)
        {
            var submission = await _context.AssignmentSubmissions
                .Include(s => s.Student)
                .Include(s => s.Assignment)
                .FirstOrDefaultAsync(s => s.Id == submissionId);

            if (submission == null || string.IsNullOrEmpty(submission.FilePath))
                return NotFound();

            var filePath = Path.Combine(_webHostEnvironment.WebRootPath, submission.FilePath.TrimStart('/'));
            var fileName = Path.GetFileName(filePath);
            return PhysicalFile(filePath, "application/octet-stream", fileName);
        }


    }
}
