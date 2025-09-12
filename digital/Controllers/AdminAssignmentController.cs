using digital.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace digital.Controllers
{
    public class AdminAssignmentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public AdminAssignmentController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public IActionResult Index()
        {
            var vm = new AdminAssignmentViewModel
            {
                Teachers = _context.TeacherMaster
                    .Select(t => new SelectListItem
                    {
                        Value = t.Id.ToString(),
                        Text = t.Teacher.Name   
                    }).ToList(),

                Categories = _context.Categories
                    .Select(c => new SelectListItem
                    {
                        Value = c.Id.ToString(),
                        Text = c.Name            
                    }).ToList(),

                SubCategories = _context.SubCategories
                    .Select(s => new SelectListItem
                    {
                        Value = s.Id.ToString(),
                        Text = s.Name            
                    }).ToList(),

                Subjects = _context.Subjects
                    .Select(su => new SelectListItem
                    {
                        Value = su.Id.ToString(),
                        Text = su.Name           
                    }).ToList()
            };

            ViewBag.AssignmentList = _context.Assignment
    .Include(a => a.Category)
    .Include(a => a.SubCategory)
    .Include(a => a.Subject)
    .ToList();


            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Index(AdminAssignmentViewModel vm, IFormFile file)
        {
            if (file != null && file.Length > 0)
            {
                var folder = Path.Combine(_env.WebRootPath, "Assignments");
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                var filePath = Path.Combine(folder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                vm.Assignment.FilePath = "/Assignments/" + fileName;
                vm.Assignment.CreatedDate = DateTime.Now; 

                _context.Assignment.Add(vm.Assignment);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }
    }
}
