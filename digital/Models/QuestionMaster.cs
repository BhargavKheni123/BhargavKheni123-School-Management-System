namespace digital.Models
{
    public class QuestionMaster
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public int SubjectId { get; set; }
        public string? ExamType { get; set; }     
        public string? QuestionText { get; set; }  
        public string? RightAnswer { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? ExamDate { get; set; }
        public int? StartHour { get; set; }
        public int? StartMinute { get; set; }
        public int? EndHour { get; set; }
        public int? EndMinute { get; set; }
        public string? Answer1 { get; set; }
        public string? Answer2 { get; set; }
        public string? Answer3 { get; set; }
        public string? Answer4 { get; set; }
        public Category Category { get; set; }
        public Subject Subject { get; set; }
        public ICollection<AnswerOptions> AnswerOptions { get; set; }
    }

   
}
