namespace digital.Models
{
    public class StudentAnswer
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public int QuestionId { get; set; }
        public string SelectedAnswer { get; set; }
        public DateTime SubmittedOn { get; set; } = DateTime.Now;

        public string ExamType { get; set; }
        public int SubjectId { get; set; }
        public int ResultId { get; set; }

        
        public Student Student { get; set; }
        public QuestionMaster Question { get; set; }
        public Subject Subject { get; set; }
    }
}
