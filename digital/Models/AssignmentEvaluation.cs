using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace digital.Models
{
    [Table("AssignmentEvaluation")]
    public class AssignmentEvaluation
    {
        [Key] 
        public int EvaluationId { get; set; }

        [ForeignKey("Assignment")]
        public int AssignmentId { get; set; }
        public virtual Assignment Assignment { get; set; }   

        public int TeacherId { get; set; }

        [StringLength(10)]
        public string? Grade { get; set; }

        public bool IsChecked { get; set; } = false;

        public DateTime? CheckedDate { get; set; }
    }
}
