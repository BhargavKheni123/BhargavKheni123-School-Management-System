using digital.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace digital.ViewModels
{
    public class ExamResultFilterViewModel
    {
        // For selected values (what user picks in the dropdowns)
        public string ExamType { get; set; }
        public int? SubjectId { get; set; }
        public int SelectedSubjectId { get; set; }
        public string SelectedExamType { get; set; }
        public string SelectedExamDate { get; set; }
        public string ExamDate { get; set; }   // or DateTime? if you are storing as DateTime

        // For dropdown lists
        public List<SelectListItem> ExamTypes { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Subjects { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Dates { get; set; } = new List<SelectListItem>();

        // For results
        public ExamResultViewModel ResultViewModel { get; set; }

        // To know if user submitted
        public bool IsSubmitted { get; set; }
    }
}
