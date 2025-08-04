namespace digital.Models
{
    public class TeacherMasterViewModel
    {
        public int Id { get; set; }

        public int CategoryId { get; set; }
        public int SubCategoryId { get; set; }
        public int SubjectId { get; set; }
        public int TeacherId { get; set; }

        public DateTime CreatedDate { get; set; }

        // For displaying related data
        public string CategoryName { get; set; }
        public string SubCategoryName { get; set; }
        public string SubjectName { get; set; }
        public string TeacherName { get; set; }
    }

}
