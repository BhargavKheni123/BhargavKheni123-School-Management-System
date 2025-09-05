using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace digital.Models
{
    [Table("Assignment")]
    public class Assignment
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        public string? Description { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [Required]
        public int SubCategoryId { get; set; }

        [Required]
        public int SubjectId { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Required]
        public DateTime SubmissionDeadline { get; set; }

        public string? FilePath { get; set; }
        public string? FileType { get; set; }
        public ICollection<AssignmentStudent> AssignmentStudents { get; set; }
    }

    [Table("AssignmentStudent")]
    public class AssignmentStudent
    {
        public int Id { get; set; }

        public int AssignmentId { get; set; }
        public Assignment Assignment { get; set; }

        public int StudentId { get; set; }
        public Student Student { get; set; }
    }
}
