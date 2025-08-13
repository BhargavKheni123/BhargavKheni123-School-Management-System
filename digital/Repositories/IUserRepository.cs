using digital.Models;

namespace digital.Repositories
{
    public interface IUserRepository
    {
        User GetUserByEmailAndPassword(string email, string password);
        User GetUserByEmail(string email);
        List<User> GetTeachers();
        void AddUser(User user);
        IEnumerable<User> GetAllUsers();
    }
}
