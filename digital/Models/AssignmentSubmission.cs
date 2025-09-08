namespace Digital.Models
{
    public class AssignmentSubmission
    {
        public int Id { get; set; }
        public int AssignmentId { get; set; }
        public int StudentId { get; set; }
        public string? FilePath { get; set; } 
        public int? Marks { get; set; }
        public DateTime SubmittedDate { get; set; }

        public Assignment Assignment { get; set; }
        public Student Student { get; set; }
    }
}
