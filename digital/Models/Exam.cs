using digital.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace digital.Models
{
    public class Exam
    {
        [Key]
        public int ExamId { get; set; }

        [Required, StringLength(200)]
        public string ExamTitle { get; set; }

        public string Description { get; set; }

        [Required, StringLength(50)]
        public string ExamType { get; set; } 

        [Required]
        public int CategoryId { get; set; } 

        [Required]
        public int SubCategoryId { get; set; } 

        [Required, StringLength(100)]
        public string Subject { get; set; }

        [Required]
        public int AssignedTeacherId { get; set; } 

        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        
        [ForeignKey("CategoryId")]
        public virtual Category Category { get; set; }

        [ForeignKey("SubCategoryId")]
        public virtual SubCategory SubCategory { get; set; }

        [ForeignKey("AssignedTeacherId")]
        public virtual TeacherMaster AssignedTeacher { get; set; }

        public virtual ICollection<ExamTeacher> ExamTeachers { get; set; }
    }
}
