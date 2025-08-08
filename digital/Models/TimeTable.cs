using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace digital.Models
{
    public class TimeTable
    {
        public int Id { get; set; }

        [Required]
        public string Std { get; set; } = "";

        [Required]
        public string Class { get; set; } = "";

        [Required]
        public string Subject { get; set; } = "";

        [Required]
        public int StartHour { get; set; }

        [Required]
        public int StartMinute { get; set; }

        [Required]
        public int EndHour { get; set; }

        [Required]
        public int EndMinute { get; set; }

        // NEW: optional teacher assignment
        public int? TeacherId { get; set; }

        [ForeignKey("TeacherId")]
        public User? Teacher { get; set; }
    }
}
