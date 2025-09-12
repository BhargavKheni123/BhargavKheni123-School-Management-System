using Digital.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace digital.Models
{
    [Table("Assignment")]
    public class Assignment
    {
        [Key]
        public int Id { get; set; }

        [StringLength(250)]
        public string? Title { get; set; }
        public string? Description { get; set; }

        public int CategoryId { get; set; }
        public int SubCategoryId { get; set; }
        public int SubjectId { get; set; }

        [Column("FilePath")]
        public string? FilePath { get; set; }

        [Column("FileType")]
        [StringLength(50)]
        public string? FileType { get; set; }

        [Column("CreatedDate")]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [Column("SubmissionDeadline")]
        public DateTime? SubmissionDeadline { get; set; }

        public int Marks { get; set; }

        [NotMapped]
        public int TeacherId { get; set; }

        [NotMapped]
        public virtual TeacherMaster? Teacher { get; set; }

        public virtual Category? Category { get; set; }
        public virtual SubCategory? SubCategory { get; set; }
        public virtual Subject? Subject { get; set; }

        public virtual ICollection<AssignmentSubmission> AssignmentSubmissions { get; set; }
            = new HashSet<AssignmentSubmission>();
    }
}
