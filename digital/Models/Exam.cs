using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace digital.Models
{
    [Table("Exams")]
    public class Exam
    {
        [Key]
        public int ExamId { get; set; }

        [Required]
        [StringLength(200)]
        public string ExamTitle { get; set; }

        public string Description { get; set; }

        [Required]
        [StringLength(100)]
        public string ExamType { get; set; }

        [Required]
        public int CategoryId { get; set; }   // Class

        [Required]
        public int SubCategoryId { get; set; }  // Division

        [Required]
        public int SubjectId { get; set; }

        [Required]
        public int AssignedTeacherId { get; set; }

        public int? CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Navigation properties (optional, for EF relationships)
        [ForeignKey("CategoryId")]
        public virtual Category Category { get; set; }

        [ForeignKey("SubCategoryId")]
        public virtual SubCategory SubCategory { get; set; }

        [ForeignKey("SubjectId")]
        public virtual Subject Subject { get; set; }

        [ForeignKey("AssignedTeacherId")]
        public virtual User AssignedTeacher { get; set; }
    }
}
