using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace digital.Models
{
    public class User
    {
        public int Id { get; set; }

        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }

        public int? StudentId { get; set; } 

        [ForeignKey("StudentId")]
        public Student Student { get; set; }
        public int? TeacherId { get; set; }
        [ForeignKey("TeacherId")]
        public TeacherMaster Teacher { get; set; }
        public string CurrentSessionId { get; set; }
        public bool IsLoggedIn { get; set; }
    }
}
