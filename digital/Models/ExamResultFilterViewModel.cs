using digital.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace digital.ViewModels
{
    public class ExamResultFilterViewModel
    {
        public string ExamType { get; set; }
        public int? SubjectId { get; set; }
        public int SelectedSubjectId { get; set; }
        public string SelectedExamType { get; set; }
        public string SelectedExamDate { get; set; }
        public string ExamDate { get; set; } 

        public List<SelectListItem> ExamTypes { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Subjects { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Dates { get; set; } = new List<SelectListItem>();

        public ExamResultViewModel ResultViewModel { get; set; }

        public bool IsSubmitted { get; set; }
    }
}
