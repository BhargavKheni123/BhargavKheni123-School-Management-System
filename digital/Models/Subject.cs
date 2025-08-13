using System.ComponentModel.DataAnnotations.Schema;

namespace digital.Models
{
    [Table("Subject")] 
    public class Subject

    {

        public int Id { get; set; }
        public string Name { get; set; }

        public ICollection<TeacherMaster> TeacherMaster { get; set; }
    }
}