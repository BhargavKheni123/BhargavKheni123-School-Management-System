using digital.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace digital.ViewModels
{
    public class AssignmentViewModel
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        public string? Description { get; set; }

        [Required]
        [Display(Name = "Standard")]
        public int CategoryId { get; set; }

        [Required]
        [Display(Name = "Class")]
        public int SubCategoryId { get; set; }

        [Required]
        public int SubjectId { get; set; }
        [Required]
        public int Marks { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Required]
        public DateTime? SubmissionDeadline { get; set; }


        public IFormFile UploadFile { get; set; }

        public string? FileType { get; set; }
        public string? FilePath { get; set; }

        public List<SelectListItem> Categories { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> SubCategories { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Subjects { get; set; } = new List<SelectListItem>();

        public List<int> SelectedStudents { get; set; } = new List<int>();
        public bool AssignAllStudents { get; set; }
        public List<SelectListItem> Students { get; set; } = new List<SelectListItem>();
        public List<Assignment> AssignmentList { get; set; } = new List<Assignment>();

    }
}
