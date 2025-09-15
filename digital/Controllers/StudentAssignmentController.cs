using digital.Repository;
using Digital.Models;
using Digital.Models.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

public class StudentAssignmentController : Controller
{
    private readonly ApplicationDbContext _ctx;
    private readonly IAssignmentSubmissionRepository _subRepo;
    private readonly IWebHostEnvironment _env;
    private readonly IConfiguration _config;
    private readonly IAssignmentRepository _assignmentRepo;
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public StudentAssignmentController(
        ApplicationDbContext ctx,
        IAssignmentSubmissionRepository subRepo,
        IWebHostEnvironment env,
        IConfiguration config,
        IAssignmentRepository assignmentRepo,
        ApplicationDbContext context,
        IWebHostEnvironment webHostEnvironment)
    {
        _ctx = ctx;
        _subRepo = subRepo;
        _env = env;
        _config = config;
        _assignmentRepo = assignmentRepo;
        _context = context;
        _webHostEnvironment = webHostEnvironment;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var student = await _ctx.Student.FirstOrDefaultAsync();
        if (student == null) return Content("No student found in DB");

        var assignments = await _ctx.Assignment
            .Where(a => a.CategoryId == student.CategoryId && a.SubCategoryId == student.SubCategoryId)
            .Include(a => a.Subject)
            .OrderByDescending(a => a.CreatedDate)
            .ToListAsync();

        var model = assignments.Select(a => new StudentAssignmentItemViewModel
        {
            Assignment = a,
            AssignmentSubmissions = _ctx.AssignmentSubmissions
                .FirstOrDefault(s => s.AssignmentId == a.Id && s.StudentId == student.Id)
        }).ToList();

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Upload(int AssignmentId, IFormFile file)
    {
        if (file == null || file.Length == 0)
            return Content("Please select a file to upload.");

        int? studentId = HttpContext.Session.GetInt32("StudentId");
        if (studentId == null)
            return Content("Session expired. Please login again.");

        var student = await _ctx.Student.FirstOrDefaultAsync(s => s.Id == studentId);
        if (student == null) return Content("Student not found in DB");

        var assignment = await _ctx.Assignment
            .Include(a => a.Subject)
            .FirstOrDefaultAsync(a => a.Id == AssignmentId);
        if (assignment == null)
            return Content("Invalid assignment.");

        var extension = Path.GetExtension(file.FileName).ToLower();
        if (assignment.FileType == "PDF" && extension != ".pdf")
            return Content("This assignment only accepts PDF uploads.");
        if (assignment.FileType == "Word" && extension != ".docx")
            return Content("This assignment only accepts Word uploads.");

        var standardName = _ctx.Categories.FirstOrDefault(c => c.Id == student.CategoryId)?.Name ?? "Standard";
        var divisionName = _ctx.SubCategories.FirstOrDefault(sc => sc.Id == student.SubCategoryId)?.Name ?? "Division";
        var studentName = student.Name.Replace(" ", "_");
        var subjectName = assignment.Subject?.Name ?? "Subject";

        var folderPath = Path.Combine(_env.WebRootPath, "Assignments", "Submissions",
                                      standardName, divisionName, studentName, subjectName);

        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        var fileName = $"Assignment_{assignment.Id}{extension}";
        var filePath = Path.Combine(folderPath, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var submission = await _ctx.AssignmentSubmissions
            .FirstOrDefaultAsync(s => s.AssignmentId == assignment.Id && s.StudentId == student.Id);

        if (submission == null)
        {
            submission = new AssignmentSubmission
            {
                AssignmentId = AssignmentId,
                StudentId = student.Id,
                FileName = fileName,
                FilePath = filePath.Replace(_env.WebRootPath, "").Replace("\\", "/"),
                SubmittedDate = DateTime.Now,
                Marks = null
            };
            _ctx.AssignmentSubmissions.Add(submission);
        }
        else
        {
            submission.FileName = fileName;
            submission.FilePath = filePath.Replace(_env.WebRootPath, "").Replace("\\", "/");
            submission.SubmittedDate = DateTime.Now;
            _ctx.AssignmentSubmissions.Update(submission);
        }

        await _ctx.SaveChangesAsync();

        TempData["Success"] = "Assignment uploaded successfully!";
        return RedirectToAction("Index");
    }

    public async Task<IActionResult> DownloadAssignment(int id)
    {
        var assignment = await _assignmentRepo.GetAssignmentByIdAsync(id);
        if (assignment == null || string.IsNullOrEmpty(assignment.FilePath))
            return NotFound();

        var filePath = Path.Combine(_env.WebRootPath, assignment.FilePath.TrimStart('/'));
        var fileName = Path.GetFileName(filePath);
        var contentType = "application/octet-stream";

        return PhysicalFile(filePath, contentType, fileName);
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
    public async Task<IActionResult> MyGrades()
    {
        int? studentId = HttpContext.Session.GetInt32("StudentId");
        if (studentId == null) return RedirectToAction("Login", "Account");

        var submissions = await _ctx.AssignmentSubmissions
            .Include(s => s.Assignment)
            .Where(s => s.StudentId == studentId)
            .OrderByDescending(s => s.SubmittedDate)
            .ToListAsync();

        return View(submissions);
    }
    [HttpPost]
    public async Task<IActionResult> SubmitAssignment(int submissionId, IFormFile file)
    {
        var submission = await _context.AssignmentSubmissions.FindAsync(submissionId);
        if (submission == null) return NotFound();

        // file save karo
        var folder = Path.Combine(_webHostEnvironment.WebRootPath, "student_submissions");
        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);

        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
        var filePath = Path.Combine(folder, fileName);
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        submission.FilePath = "/student_submissions/" + fileName;
        submission.SubmittedDate = DateTime.Now; 
        await _context.SaveChangesAsync();

        return RedirectToAction("MyAssignments");
    }



}


