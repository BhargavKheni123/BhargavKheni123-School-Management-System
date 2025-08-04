using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace digital.Models
{
    public class StudentViewModel
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [Display(Name = "Standard")]
        public int CategoryId { get; set; }

        [Required]
        [Display(Name = "Class")]
        public int SubCategoryId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime DOB { get; set; }

        [Required]
        public string Gender { get; set; }

        [Required]
        [Display(Name = "Mobile Number")]
        public string MobileNumber { get; set; }

        [Required]
        public string Address { get; set; }

        public DateTime CreatedDate { get; set; }

        // Dropdowns
        public List<SelectListItem> Categories { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> SubCategories { get; set; } = new List<SelectListItem>();

        // Display list
        public List<Student> StudentList { get; set; } = new List<Student>();
    }
}
