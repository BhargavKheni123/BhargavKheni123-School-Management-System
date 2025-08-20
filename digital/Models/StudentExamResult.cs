namespace digital.Models
{
    public class StudentExamResult
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public int SubjectId { get; set; }
        public string ExamType { get; set; }
        public int TotalQuestions { get; set; }
        public int CorrectAnswers { get; set; }
        public DateTime SubmittedOn { get; set; } = DateTime.Now;

        public Student Student { get; set; }
        public Subject Subject { get; set; }
    }
}
