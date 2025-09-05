namespace digital.Models
{
    public class AssignmentSubmission
    {
        public int Id { get; set; }
        public int AssignmentId { get; set; }
        public int StudentId { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; } 
        public DateTime UploadedDate { get; set; }
        public int? Marks { get; set; }
        public string Remarks { get; set; }

       // public virtual Assignment Assignment { get; set; }
        public virtual Student Student { get; set; }
    }

}
