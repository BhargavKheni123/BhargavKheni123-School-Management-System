using Digital.Models;
using System.ComponentModel.DataAnnotations.Schema;

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
namespace digital.Models
{
    public class Assignment
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }

        [Column("FilePath")]
        public string FilePath { get; set; }

        [Column("CreatedDate")]
        public DateTime CreatedDate { get; set; }

        [Column("SubmissionDeadline")]
        public DateTime? SubmissionDeadline { get; set; }

        [Column("FileType")]
        public string FileType { get; set; }

        public int Marks { get; set; }

        public int TeacherId { get; set; }
        public int CategoryId { get; set; }
        public int SubCategoryId { get; set; }
        public int SubjectId { get; set; }

        public TeacherMaster Teacher { get; set; }
        public Category Category { get; set; }
        public SubCategory SubCategory { get; set; }
        public Subject Subject { get; set; }
        public ICollection<AssignmentSubmission> AssignmentSubmissions { get; set; }

    }
}
