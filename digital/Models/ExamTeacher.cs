using digital.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace digital.Models
{
    public class ExamTeacher
    {
        [Key]
        public int ExamTeacherId { get; set; }

        [Required]
        public int ExamId { get; set; }

        [Required]
        public int TeacherId { get; set; }

        public DateTime AssignedDate { get; set; } = DateTime.Now;

        
        [ForeignKey("ExamId")]
        public virtual Exam Exam { get; set; }

        [ForeignKey("TeacherId")]
        public virtual TeacherMaster Teacher { get; set; }
    }
}
