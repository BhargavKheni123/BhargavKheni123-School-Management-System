using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace digital.ViewModels
{
    public class ExamViewModel
    {
        public int ExamId { get; set; }

        [Required(ErrorMessage = "Exam Title is required")]
        public string ExamTitle { get; set; }

        public string Description { get; set; }

        [Required(ErrorMessage = "Exam Type is required")]
        public string ExamType { get; set; }

        [Required(ErrorMessage = "Class is required")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Division is required")]
        public int SubCategoryId { get; set; }

        [Required(ErrorMessage = "Subject is required")]
        public int SubjectId { get; set; }

        [Required(ErrorMessage = "Assigned Teacher is required")]
        public int AssignedTeacherId { get; set; }

        // Dropdown lists
        public IEnumerable<SelectListItem> Categories { get; set; }
        public IEnumerable<SelectListItem> SubCategories { get; set; }
        public IEnumerable<SelectListItem> Subjects { get; set; }
        public IEnumerable<SelectListItem> Teachers { get; set; }
        public List<ExamListItem> ExamList { get; set; }
    }

    public class ExamListItem
    {
        public int ExamId { get; set; }
        public string ExamTitle { get; set; }
        public string ExamType { get; set; }
        public string ClassName { get; set; }
        public string DivisionName { get; set; }
        public string SubjectName { get; set; }
        public string TeacherName { get; set; }
        public int AssignedTeacherId { get; set; }
    }

}

