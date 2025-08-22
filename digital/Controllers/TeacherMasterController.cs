using digital.Interfaces;
using digital.Models;
using digital.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace digital.Controllers
{
    public class TeacherMasterController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly ITeacherMasterRepository _teacherMasterRepository;

        public TeacherMasterController(
            IUserRepository userRepository,
            ITeacherMasterRepository teacherMasterRepository)
        {
            _userRepository = userRepository;
            _teacherMasterRepository = teacherMasterRepository;
        }

        public IActionResult TeacherMaster()
        {
            ViewBag.Categories = _teacherMasterRepository.GetCategories();
            ViewBag.Subjects = _teacherMasterRepository.GetSubjects();
            ViewBag.Teachers = _userRepository.GetTeachers();

            var data = _teacherMasterRepository.GetAllWithRelations();
            return View("TeacherMaster", data);
        }

        [HttpGet]
        public IActionResult GetSubCategories(int categoryId)
        {
            var subCategories = _teacherMasterRepository.GetSubCategoriesByCategory(categoryId)
                .Select(sc => new { sc.Id, sc.Name }).ToList();

            return Json(subCategories);
        }

        [HttpGet]
        public IActionResult GetSubCategoriesbyteacher(int categoryId)
        {
            var subCategories = _teacherMasterRepository.GetSubCategoriesByCategory(categoryId)
                .Select(sc => new { sc.Id, sc.Name }).ToList();

            return Json(subCategories);
        }

        [HttpPost]
        public IActionResult SaveTeacherAssignment(int CategoryId, int SubCategoryId, int SubjectId, int TeacherId)
        {
            if (!_teacherMasterRepository.Exists(CategoryId, SubCategoryId, SubjectId, TeacherId))
            {
                var newItem = new TeacherMaster
                {
                    CategoryId = CategoryId,
                    SubCategoryId = SubCategoryId,
                    SubjectId = SubjectId,
                    TeacherId = TeacherId,
                    CreatedDate = DateTime.Now
                };

                _teacherMasterRepository.Add(newItem);
            }

            return RedirectToAction("TeacherMaster");
        }

        public IActionResult EditTeacherMaster(int id)
        {
            var item = _teacherMasterRepository.GetById(id);
            if (item == null)
                return NotFound();

            ViewBag.Categories = _teacherMasterRepository.GetCategories();
            ViewBag.Subjects = _teacherMasterRepository.GetSubjects();
            ViewBag.Teachers = _teacherMasterRepository.GetTeachers();
            ViewBag.SubCategories = _teacherMasterRepository.GetSubCategoriesByCategory(item.CategoryId);

            return View("EditTeacherMaster", item);
        }

        [HttpPost]
        public IActionResult UpdateTeacherMaster(TeacherMaster model)
        {
            var existing = _teacherMasterRepository.GetById(model.Id);
            if (existing == null)
                return NotFound();

            existing.CategoryId = model.CategoryId;
            existing.SubCategoryId = model.SubCategoryId;
            existing.SubjectId = model.SubjectId;
            existing.TeacherId = model.TeacherId;

            _teacherMasterRepository.Update(existing);

            return RedirectToAction("TeacherMaster");
        }

        public IActionResult DeleteTeacherMaster(int id)
        {
            _teacherMasterRepository.Delete(id);
            return RedirectToAction("TeacherMaster");
        }

    }
}
