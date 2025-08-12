using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

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

        [Required(ErrorMessage = "Please select a teacher")]
        public int AssignedTeacherId { get; set; } 

        public List<int> SelectedTeacherIds { get; set; } = new List<int>();

        public List<SelectListItem> Classes { get; set; } = new List<SelectListItem>();

        public List<SelectListItem> Divisions { get; set; } = new List<SelectListItem>();

        public List<SelectListItem> Subjects { get; set; } = new List<SelectListItem>();

        public List<SelectListItem> Teachers { get; set; } = new List<SelectListItem>();

        public List<SelectListItem> ExamTypes { get; set; } = new List<SelectListItem>();
    }
}
