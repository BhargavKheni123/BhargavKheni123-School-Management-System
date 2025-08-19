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

        public Category Category { get; set; }
        public Subject Subject { get; set; }
        public ICollection<AnswerOptions> AnswerOptions { get; set; }
    }

   
}
