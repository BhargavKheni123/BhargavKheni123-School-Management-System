using digital.Models;
using Digital.Models;

public class Assignment
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public int CategoryId { get; set; }
    public int SubCategoryId { get; set; }
    public int SubjectId { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? SubmissionDeadline { get; set; }
    public string? FilePath { get; set; }
    public string FileType { get; set; }
    public int Marks { get; set; }

    
    public Subject Subject { get; set; }
    public ICollection<AssignmentSubmission> Submission { get; set; }

}
