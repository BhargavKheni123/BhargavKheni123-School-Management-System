using digital.Models;
using digital.ViewModels;
using digital.Repository;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace digital.Controllers
{
    public class TeacherController : Controller
    {
        private readonly ITeacherRepository _repository;

        public TeacherController(ITeacherRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public IActionResult TeacherRegister()
        {
            ViewBag.TeacherList = _repository.GetAllTeachers().ToList();
            return View(new TeacherViewModel());
        }

        [HttpPost]
        public IActionResult TeacherRegister(TeacherViewModel model)
        {
            if (ModelState.IsValid)
            {
                
                var teacher = new Teacher
                {
                    Name = model.Name,
                    Email = model.Email,
                    Password = model.Password,
                    DOB = model.DOB,
                    Gender = model.Gender,
                    Address = model.Address
                };

                _repository.AddTeacher(teacher);
                _repository.Save(); 

                
                var user = new User
                {
                    Name = teacher.Name,
                    Email = teacher.Email,
                    Password = teacher.Password,
                    Role = "Teacher",
                    TeacherId = teacher.TeacherId,
                    CurrentSessionId = _repository.GetCurrentSessionId().ToString() 
                };

                _repository.AddUser(user);
                _repository.Save();

                return RedirectToAction("TeacherRegister", "Teacher");
            }

            ViewBag.TeacherList = _repository.GetAllTeachers().ToList();
            return View(model);
        }


        [HttpGet]
        public IActionResult Edit(int id)
        {
            var teacher = _repository.GetTeacherById(id);
            if (teacher == null)
            {
                return NotFound();
            }

            var model = new TeacherViewModel
            {
                Name = teacher.Name,
                Email = teacher.Email,
                Password = teacher.Password,
                DOB = teacher.DOB,
                Gender = teacher.Gender,
                Address = teacher.Address
            };

            ViewBag.TeacherId = id;
            return View(model);
        }

        [HttpPost]
        public IActionResult Edit(int id, TeacherViewModel model)
        {
            if (ModelState.IsValid)
            {
                var teacher = _repository.GetTeacherById(id);
                if (teacher == null)
                {
                    return NotFound();
                }

                teacher.Name = model.Name;
                teacher.Email = model.Email;
                teacher.Password = model.Password;
                teacher.DOB = model.DOB;
                teacher.Gender = model.Gender;
                teacher.Address = model.Address;

                _repository.UpdateTeacher(teacher);

                var user = _repository.GetUserByTeacherId(id);
                if (user != null)
                {
                    user.Name = model.Name;
                    user.Email = model.Email;
                    user.Password = model.Password;
                    _repository.UpdateUser(user);
                }

                _repository.Save();
                return RedirectToAction("TeacherRegister");
            }

            ViewBag.TeacherId = id;
            return View(model);
        }

        public IActionResult Delete(int id)
        {
            _repository.DeleteTeacher(id);
            _repository.DeleteUser(id);
            _repository.Save();

            return RedirectToAction("TeacherRegister");
        }
    }
}
