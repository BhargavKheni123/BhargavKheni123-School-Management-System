using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace digital.Models
{
    public class AnswerOptions
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int QuestionId { get; set; }

        [Required]
        public string AnswerText { get; set; }

        [Required]
        public string OptionText { get; set; }

        [Required]
        public bool IsCorrect { get; set; }

        [ForeignKey("QuestionId")]
        public QuestionMaster Question { get; set; }
    }
}
