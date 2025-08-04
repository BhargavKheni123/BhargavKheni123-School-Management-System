using digital.Interfaces;
using digital.Models;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace digital.Repositories
{
    public class StudentRepository : IStudentRepository
    {
        private readonly ApplicationDbContext _context;

        public StudentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public Student GetStudentById(int id)
        {
            return _context.Student.FirstOrDefault(s => s.Id == id);
        }

        public List<Student> GetAllStudentsWithCategoryAndSubCategory()
        {
            return _context.Student
                .Include(s => s.Category)
                .Include(s => s.SubCategory)
                .ToList();
        }

        public void AddStudent(Student student)
        {
            _context.Student.Add(student);
            _context.SaveChanges();
        }
    }
}
