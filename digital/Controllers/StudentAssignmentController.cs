using digital.Repository;
using Digital.Models;
using Digital.Models.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
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

    public StudentAssignmentController(
        ApplicationDbContext ctx,
        IAssignmentSubmissionRepository subRepo,
        IWebHostEnvironment env,
        IConfiguration config,
        IAssignmentRepository assignmentRepo)
    {
        _ctx = ctx;
        _subRepo = subRepo;
        _env = env;
        _config = config;
        _assignmentRepo = assignmentRepo;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var student = await _ctx.Student.FirstOrDefaultAsync();
        if (student == null) return Content("No student found in DB");

        var assignments = await _ctx.AssignmentSubmissions
            .Where(s => s.StudentId == student.Id)
            .Include(s => s.Assignment)
                .ThenInclude(a => a.Subject)
            .OrderByDescending(s => s.Assignment.CreatedDate)
            .ToListAsync();

        var model = assignments.Select(s => new StudentAssignmentItemViewModel
        {
            Assignment = s.Assignment,
            AssignmentSubmissions = s
        }).ToList();

        return View(model);
    }




    [HttpPost]
public async Task<IActionResult> Upload(int AssignmentId, IFormFile file)
{
    if (file == null || file.Length == 0)
        return Content("Please select a file to upload.");

    // ✅ 1. Get logged-in student
    int? studentId = HttpContext.Session.GetInt32("StudentId");
    if (studentId == null)
        return Content("Session expired. Please login again.");

    var student = await _ctx.Student.FirstOrDefaultAsync(s => s.Id == studentId);
    if (student == null) return Content("Student not found in DB");

    // ✅ 2. Get assignment
    var assignment = await _ctx.Assignment
        .Include(a => a.Subject)
        .FirstOrDefaultAsync(a => a.Id == AssignmentId);
    if (assignment == null)
        return Content("Invalid assignment.");

    // ✅ 3. File type check
    var extension = Path.GetExtension(file.FileName).ToLower();
    if (assignment.FileType == "PDF" && extension != ".pdf")
        return Content("This assignment only accepts PDF uploads.");
    if (assignment.FileType == "Word" && extension != ".docx")
        return Content("This assignment only accepts Word uploads.");

    // ✅ 4. Build folder path
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

    // ✅ 5. Save / Update DB record
    var submission = await _ctx.AssignmentSubmissions
        .FirstOrDefaultAsync(s => s.AssignmentId == assignment.Id && s.StudentId == student.Id);

        if (submission == null)
        {
            submission = new AssignmentSubmission
            {
                AssignmentId = AssignmentId,
                StudentId = student.Id,
                FileName = "Pending",
                FilePath = filePath.Replace(_env.WebRootPath, "").Replace("\\", "/"),
                SubmittedDate = DateTime.Now
               
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



}
