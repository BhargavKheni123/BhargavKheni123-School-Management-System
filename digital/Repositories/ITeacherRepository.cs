using digital.Models;
using System.Collections.Generic;

namespace digital.Repository
{
    public interface ITeacherRepository
    {
        IEnumerable<Teacher> GetAllTeachers();
        Teacher GetTeacherById(int id);
        void AddTeacher(Teacher teacher);
        void UpdateTeacher(Teacher teacher);
        void DeleteTeacher(int id);

        void AddUser(User user);
        User GetUserByTeacherId(int teacherId);
        void UpdateUser(User user);
        void DeleteUser(int teacherId);
        int GetCurrentSessionId();
        void Save();
    }
}
