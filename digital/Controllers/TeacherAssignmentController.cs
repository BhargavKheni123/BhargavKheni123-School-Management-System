using Digital.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

public class TeacherAssignmentController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _env;

    public TeacherAssignmentController(ApplicationDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    // Teacher Dashboard - sab submissions dekhne ke liye
    public async Task<IActionResult> Index()
    {
        var submissions = await _context.AssignmentSubmissions
            .Include(s => s.Assignment)
            .Include(s => s.Student)
            .OrderByDescending(s => s.SubmittedDate)
            .ToListAsync();

        return View(submissions);
    }

    // Download file jo student ne upload kiya
    public async Task<IActionResult> DownloadSubmission(int id)
    {
        var submission = await _context.AssignmentSubmissions.FindAsync(id);
        if (submission == null) return NotFound();

        var filePath = _env.WebRootPath + submission.FilePath;
        var fileName = submission.FileName;
        return PhysicalFile(filePath, "application/octet-stream", fileName);
    }

    // Teacher grading karega
    [HttpPost]
    public async Task<IActionResult> GradeSubmission(int submissionId, int marks)
    {
        var submission = await _context.AssignmentSubmissions
            .Include(s => s.Assignment)
            .FirstOrDefaultAsync(s => s.Id == submissionId);

        if (submission == null) return NotFound();

        submission.Marks = marks;
        await _context.SaveChangesAsync();

        TempData["Success"] = "Grade assigned successfully!";
        return RedirectToAction("Index");
    }
}
