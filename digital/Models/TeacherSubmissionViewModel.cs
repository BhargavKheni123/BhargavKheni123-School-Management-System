using digital.Models;
using System;
using System.Collections.Generic;

namespace digital.ViewModels
{
    public class TeacherSubmissionViewModel
    {
        public int AssignmentId { get; set; }
        public string AssignmentTitle { get; set; }

        public List<SubmissionWithEvaluation> Submissions { get; set; }
    }

    public class SubmissionWithEvaluation
    {
        public int SubmissionId { get; set; }           
        public int StudentId { get; set; }
        public string FilePath { get; set; }
        public DateTime? SubmissionDate { get; set; }
        public decimal? Marks { get; set; } 
        public decimal? TotalMarks { get; set; }

        public string? Grade { get; set; }
        public bool IsChecked { get; set; }
    }
    public class TeacherSubmissionWithMarks
    {
        public int SubmissionId { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; }
        public string FilePath { get; set; }
        public DateTime? SubmittedDate { get; set; }
        public decimal? Marks { get; set; }
        public decimal? TotalMarks { get; set; } 
    }

}

