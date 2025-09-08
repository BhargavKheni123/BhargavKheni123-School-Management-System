using digital.Models;
using digital.Repository;
using digital.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
                Subjects = _context.Subjects.Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name }).ToList()
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
                string uniqueFileName = null;

                if (vm.UploadFile != null)
                {
                    string wwwRootPath = _webHostEnvironment.WebRootPath;

                    string folderPath = Path.Combine(wwwRootPath, "uploads",
                    vm.CategoryId.ToString(),
                    vm.SubCategoryId.ToString(),
                    vm.SubjectId.ToString());

                    if (!Directory.Exists(folderPath))
                        Directory.CreateDirectory(folderPath);


                    uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(vm.UploadFile.FileName);
                    string filePath = Path.Combine(folderPath, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await vm.UploadFile.CopyToAsync(stream);
                    }
                }

                var assignment = new Assignment
                {
                    Title = vm.Title,
                    Description = vm.Description,
                    CategoryId = vm.CategoryId,
                    SubCategoryId = vm.SubCategoryId,
                    SubjectId = vm.SubjectId,
                    SubmissionDeadline = vm.SubmissionDeadline,
                    FilePath = uniqueFileName != null
                        ? Path.Combine("uploads", vm.CategoryId.ToString(), vm.SubCategoryId.ToString(), vm.SubjectId.ToString(), uniqueFileName)
                        : null,
                    CreatedDate = DateTime.Now
                };

                _context.Assignment.Add(assignment);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Create));
            

        }





        public async Task<IActionResult> Index()
        {
            var assignments = await _repository.GetAllAssignmentsAsync();
            return View(assignments);
        }
    }
}
