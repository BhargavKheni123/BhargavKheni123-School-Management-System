using digital.Models;
using digital.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace digital.Controllers
{
    public class TeacherController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TeacherController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult TeacherRegister()
        {
            ViewBag.TeacherList = _context.Teachers.ToList(); 
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

                _context.Teachers.Add(teacher);
                _context.SaveChanges();

                var user = new User
                {
                    Name = teacher.Name,
                    Email = teacher.Email,
                    Password = teacher.Password,
                    Role = "Teacher",
                    TeacherId = teacher.TeacherId
                };

                _context.Users.Add(user);
                _context.SaveChanges();

                return RedirectToAction("TeacherRegister", "Teacher");
            }
            ViewBag.TeacherList = _context.Teachers.ToList();
            return View(model);
        }
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var teacher = _context.Teachers.FirstOrDefault(t => t.TeacherId == id);
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
                var teacher = _context.Teachers.FirstOrDefault(t => t.TeacherId == id);
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

                var user = _context.Users.FirstOrDefault(u => u.TeacherId == id);
                if (user != null)
                {
                    user.Name = model.Name;
                    user.Email = model.Email;
                    user.Password = model.Password;
                }

                _context.SaveChanges();
                return RedirectToAction("TeacherRegister");
            }

            ViewBag.TeacherId = id;
            return View(model);
        }


        public IActionResult Delete(int id)
        {
            var teacher = _context.Teachers.Find(id);
            if (teacher != null)
            {
                _context.Teachers.Remove(teacher);

                var user = _context.Users.FirstOrDefault(u => u.TeacherId == id);
                if (user != null) _context.Users.Remove(user);

                _context.SaveChanges();
            }
            return RedirectToAction("TeacherRegister");
        }
    }
}

