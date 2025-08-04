using digital.Models;
using System.Collections.Generic;

namespace digital.Interfaces
{
    public interface IStudentRepository
    {
        Student GetStudentById(int id);
        List<Student> GetAllStudentsWithCategoryAndSubCategory();

        void AddStudent(Student student);
    }
}
