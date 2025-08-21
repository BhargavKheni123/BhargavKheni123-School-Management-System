using Microsoft.AspNetCore.Mvc.Rendering;

namespace digital.Models
{
    public class TeacherMasterViewModel
    {
        public int Id { get; set; }

        public int CategoryId { get; set; }
        public int SubCategoryId { get; set; }
        public int SubjectId { get; set; }
        public string TeacherId { get; set; }

        public DateTime CreatedDate { get; set; }

        public string CategoryName { get; set; }
        public string SubCategoryName { get; set; }
        public string SubjectName { get; set; }
        public string TeacherName { get; set; }
    }

}
