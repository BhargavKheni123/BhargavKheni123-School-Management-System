using digital.Models;
using System.Collections.Generic;
using System.Linq;

namespace digital.Repository
{
    public class TeacherRepository : ITeacherRepository
    {
        private readonly ApplicationDbContext _context;

        public TeacherRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public int GetCurrentSessionId()
        {
            return 1; 
        }

        public IEnumerable<Teacher> GetAllTeachers()
        {
            return _context.Teachers.ToList();
        }

        public Teacher GetTeacherById(int id)
        {
            return _context.Teachers.FirstOrDefault(t => t.TeacherId == id);
        }

        public void AddTeacher(Teacher teacher)
        {
            _context.Teachers.Add(teacher);
        }

        public void UpdateTeacher(Teacher teacher)
        {
            _context.Teachers.Update(teacher);
        }

        public void DeleteTeacher(int id)
        {
            var teacher = _context.Teachers.Find(id);
            if (teacher != null)
            {
                _context.Teachers.Remove(teacher);
            }
        }

        public void AddUser(User user)
        {
            _context.Users.Add(user);
        }

        public User GetUserByTeacherId(int teacherId)
        {
            return _context.Users.FirstOrDefault(u => u.TeacherId == teacherId);
        }

        public void UpdateUser(User user)
        {
            _context.Users.Update(user);
        }

        public void DeleteUser(int teacherId)
        {
            var user = _context.Users.FirstOrDefault(u => u.TeacherId == teacherId);
            if (user != null)
            {
                _context.Users.Remove(user);
            }
        }

        public void Save()
        {
            _context.SaveChanges();
        }
    }
}
