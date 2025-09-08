using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Digital.Models;
using Digital.Models.ViewModels;

public class StudentAssignmentController : Controller
{
    private readonly ApplicationDbContext _ctx;
    private readonly IAssignmentSubmissionRepository _subRepo;
    private readonly IWebHostEnvironment _env;
    private readonly IConfiguration _config;

    public StudentAssignmentController(
        ApplicationDbContext ctx,
        IAssignmentSubmissionRepository subRepo,
        IWebHostEnvironment env,
        IConfiguration config)
    {
        _ctx = ctx;
        _subRepo = subRepo;
        _env = env;
        _config = config;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        // abhi fake student bana ke return kar do
        var student = await _ctx.Student.FirstOrDefaultAsync(); // koi bhi ek student le aayega
        if (student == null) return Content("No student found in DB");

        var assignments = await _ctx.Assignment
            .Where(a => a.CategoryId == student.CategoryId && a.SubCategoryId == student.SubCategoryId)
            .Include(a => a.Subject)
            .OrderByDescending(a => a.CreatedDate)
            .ToListAsync();

        var model = assignments.Select(a => new StudentAssignmentItemViewModel
        {
            Assignment = a,
            Submission = _ctx.AssignmentSubmissions.FirstOrDefault(s => s.AssignmentId == a.Id && s.StudentId == student.Id)
        }).ToList();

        return View(model);
    }



    [HttpPost]
    public async Task<IActionResult> Upload(int AssignmentId, IFormFile file)
    {
        if (file == null || file.Length == 0)
            return Content("Please select a file");

        // koi bhi ek student use karo
        var student = await _ctx.Student.FirstOrDefaultAsync();
        var assignment = await _ctx.Assignment.Include(a => a.Subject).FirstOrDefaultAsync(a => a.Id == AssignmentId);

        if (assignment == null || student == null)
            return Content("Invalid assignment or student");

        var root = Path.Combine(_env.WebRootPath, "Assignments");
        var folderPath = Path.Combine(root, "TempUploads");

        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        var fileName = Path.GetFileName(file.FileName);
        var filePath = Path.Combine(folderPath, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var submission = new AssignmentSubmission
        {
            AssignmentId = AssignmentId,
            StudentId = student.Id,
            FilePath = filePath.Replace(_env.WebRootPath, ""),
            SubmittedAt = DateTime.Now
        };

        _ctx.AssignmentSubmissions.Add(submission);
        await _ctx.SaveChangesAsync();

        return RedirectToAction("Index");
    }

}
