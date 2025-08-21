using digital.Interfaces;
using digital.Models;
using digital.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace digital.Controllers
{
    public class TeacherMasterController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserRepository _userRepository;
        private readonly ITeacherMasterRepository _teacherMasterRepository;

        public TeacherMasterController(
            ApplicationDbContext context,
            IUserRepository userRepository,
            ITeacherMasterRepository teacherMasterRepository)
        {
            _context = context;
            _userRepository = userRepository;
            _teacherMasterRepository = teacherMasterRepository;
        }

        public IActionResult TeacherMaster()
        {
            ViewBag.Categories = _context.Categories.ToList();
            ViewBag.Subjects = _context.Subjects.ToList();
            ViewBag.Teachers = _userRepository.GetTeachers();

            var data = _teacherMasterRepository.GetAllWithRelations();
            return View("TeacherMaster", data);
        }
        [HttpGet]
        public IActionResult GetSubCategories(int categoryId)
        {
            var subCategories = _context.SubCategories
                .Where(sc => sc.CategoryId == categoryId)
                .Select(sc => new { sc.Id, sc.Name })
                .ToList();

            return Json(subCategories);
        }

        [HttpGet]
        public IActionResult GetSubCategoriesbyteacher(int categoryId)
        {
            var subCategories = _context.SubCategories
                .Where(sc => sc.CategoryId == categoryId)
                .Select(sc => new { sc.Id, sc.Name })
                .ToList();

            return Json(subCategories);
        }

        [HttpPost]
        public IActionResult SaveTeacherAssignment(int CategoryId, int SubCategoryId, int SubjectId, int TeacherId)
        {
            var exists = _context.TeacherMaster.FirstOrDefault(x =>
                x.CategoryId == CategoryId &&
                x.SubCategoryId == SubCategoryId &&
                x.SubjectId == SubjectId &&
                x.TeacherId == TeacherId);

            if (exists == null)
            {
                var newItem = new TeacherMaster
                {
                    CategoryId = CategoryId,
                    SubCategoryId = SubCategoryId,
                    SubjectId = SubjectId,
                    TeacherId = TeacherId,
                    CreatedDate = DateTime.Now
                };

                _context.TeacherMaster.Add(newItem);
                _context.SaveChanges();
            }

            return RedirectToAction("TeacherMaster");
        }

        public IActionResult EditTeacherMaster(int id)
        {
            var item = _context.TeacherMaster
                .Include(x => x.Category)
                .Include(x => x.SubCategory)
                .Include(x => x.Subject)
                .Include(x => x.Teacher)
                .FirstOrDefault(x => x.Id == id);

            if (item == null)
                return NotFound();

            ViewBag.Categories = _context.Categories.ToList();
            ViewBag.Subjects = _context.Subjects.ToList();
            ViewBag.Teachers = _context.Users.Where(u => u.Role == "Teacher").ToList();
            ViewBag.SubCategories = _context.SubCategories
                .Where(s => s.CategoryId == item.CategoryId)
                .ToList();

            return View("EditTeacherMaster", item);
        }

        [HttpPost]
        public IActionResult UpdateTeacherMaster(TeacherMaster model)
        {
            var existing = _context.TeacherMaster.FirstOrDefault(x => x.Id == model.Id);
            if (existing == null)
                return NotFound();

            existing.CategoryId = model.CategoryId;
            existing.SubCategoryId = model.SubCategoryId;
            existing.SubjectId = model.SubjectId;
            existing.TeacherId = model.TeacherId;

            _context.SaveChanges();
            return RedirectToAction("TeacherMaster");
        }

        public IActionResult DeleteTeacherMaster(int id)
        {
            var item = _context.TeacherMaster.Find(id);
            if (item != null)
            {
                _context.TeacherMaster.Remove(item);
                _context.SaveChanges();
            }
            return RedirectToAction("TeacherMaster");
        }
        public List<TeacherMaster> GetAllWithRelations()
        {
            return _context.TeacherMaster
                .Include(t => t.Category)
                .Include(t => t.SubCategory)
                .Include(t => t.Subject)
                .Include(t => t.Teacher)
                .OrderByDescending(t => t.CreatedDate)
                .ToList();
        }
    }
}