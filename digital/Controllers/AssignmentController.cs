using digital.Models;
using digital.Repository;
using digital.ViewModels;
using DocumentFormat.OpenXml.Packaging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Presentation;
using A = DocumentFormat.OpenXml.Drawing;
using SixLabors.ImageSharp.Drawing.Processing; 
using SixLabors.Fonts;




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
            if (vm.SubmissionDeadline < new DateTime(1753, 1, 1))
            {
                ModelState.AddModelError("SubmissionDeadline", "Invalid date");
                return View(vm);
            }

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
            else if (vm.FileType == "Image")
            {
                filePath = Path.Combine(folderPath, uniqueFileName + ".png");

                using (var image = new SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32>(800, 600))
                {
                    image.Mutate(ctx =>
                    {
                        ctx.Fill(SixLabors.ImageSharp.Color.White); 

                        int y = 20;
                        var font = SixLabors.Fonts.SystemFonts.CreateFont("Arial", 20);

                        ctx.DrawText($"Title: {vm.Title}", font, SixLabors.ImageSharp.Color.Black, new SixLabors.ImageSharp.PointF(20, y));
                        y += 40;
                        ctx.DrawText($"Description: {vm.Description}", font, SixLabors.ImageSharp.Color.Black, new SixLabors.ImageSharp.PointF(20, y));
                        y += 40;
                        ctx.DrawText($"Standard: {vm.CategoryId}", font, SixLabors.ImageSharp.Color.Black, new SixLabors.ImageSharp.PointF(20, y));
                        y += 40;
                        ctx.DrawText($"Class: {vm.SubCategoryId}", font, SixLabors.ImageSharp.Color.Black, new SixLabors.ImageSharp.PointF(20, y));
                        y += 40;
                        ctx.DrawText($"Subject: {vm.SubjectId}", font, SixLabors.ImageSharp.Color.Black, new SixLabors.ImageSharp.PointF(20, y));
                        y += 40;
                        ctx.DrawText($"Marks: {vm.Marks}", font, SixLabors.ImageSharp.Color.Black, new SixLabors.ImageSharp.PointF(20, y));
                        y += 40;
                        ctx.DrawText($"Deadline: {vm.SubmissionDeadline:dd-MM-yyyy}", font, SixLabors.ImageSharp.Color.Black, new SixLabors.ImageSharp.PointF(20, y));
                    });

                    await image.SaveAsPngAsync(filePath);
                }
            }

            else if (vm.FileType == "PPT")
            {
                filePath = Path.Combine(folderPath, uniqueFileName + ".pptx");

                using (var ppt = PresentationDocument.Create(filePath, PresentationDocumentType.Presentation))
                {
                    var presentationPart = ppt.AddPresentationPart();
                    presentationPart.Presentation = new Presentation();

                    var slidePart = presentationPart.AddNewPart<SlidePart>();
                    slidePart.Slide = new Slide(new CommonSlideData(new ShapeTree()));

                    var slideIdList = presentationPart.Presentation.AppendChild(new SlideIdList());
                    uint slideId = 256;
                    slideIdList.Append(new SlideId
                    {
                        Id = slideId,
                        RelationshipId = presentationPart.GetIdOfPart(slidePart)
                    });

                    var notes = $"Title: {vm.Title}\n" +
                                $"Description: {vm.Description}\n" +
                                $"Standard: {vm.CategoryId}\n" +
                                $"Class: {vm.SubCategoryId}\n" +
                                $"Subject: {vm.SubjectId}\n" +
                                $"Marks: {vm.Marks}\n" +
                                $"Deadline: {vm.SubmissionDeadline:dd-MM-yyyy}";

                    var shapeTree = slidePart.Slide.CommonSlideData.ShapeTree;
                    var shape = new Shape(
                        new NonVisualShapeProperties(
                            new NonVisualDrawingProperties() { Id = 2, Name = "ContentBox" },
                            new NonVisualShapeDrawingProperties(new A.ShapeLocks() { NoGrouping = true }),
                            new ApplicationNonVisualDrawingProperties()
                        ),
                        new ShapeProperties(),
                        new TextBody(
                            new A.BodyProperties(),
                            new A.ListStyle(),
                            new A.Paragraph(new A.Run(new A.Text(notes)))
                        )
                    );

                    shapeTree.Append(shape);

                    presentationPart.Presentation.Save();
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
                SubmissionDeadline = vm.SubmissionDeadline ?? DateTime.Now,
                FilePath = string.IsNullOrEmpty(filePath) ? "" : filePath.Replace(wwwRootPath, "").Replace("\\", "/"),
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
                //SubmissionDeadline = assignment.SubmissionDeadline,
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
            assignment.SubmissionDeadline = vm.SubmissionDeadline ?? DateTime.Now;

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

        private string GetGradeLetter(decimal? obtained, decimal? total)
        {
            if (!obtained.HasValue) return "Not Submitted";
            if (!total.HasValue || total.Value == 0) return "-";

            decimal percent = (obtained.Value / total.Value) * 100;

            if (percent >= 90) return "A+";
            if (percent >= 80) return "A";
            if (percent >= 70) return "B";
            if (percent >= 60) return "C";
            if (percent >= 50) return "D";
            return "F";
        }

        [HttpGet]
        public async Task<IActionResult> TeacherSubmissions(int assignmentId)
        {
            var submissions = await _context.AssignmentSubmissions
                .Include(s => s.Student)
                .Include(s => s.Assignment)
                .Where(s => s.AssignmentId == assignmentId)
                .ToListAsync();

            return View(submissions);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateMarks(int submissionId, decimal obtainedMarks)
        {
            var submission = await _context.AssignmentSubmissions
                .Include(s => s.Assignment)
                .FirstOrDefaultAsync(s => s.Id == submissionId);

            if (submission == null) return NotFound();

            submission.Marks = obtainedMarks;
            await _context.SaveChangesAsync();

            var grade = GetGradeLetter(submission.Marks, submission.Assignment.Marks);

            return Json(new { success = true, grade });
        }
    }
}
