using digital.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace digital.ViewModels
{
    public class AttendanceViewModel
    {
        public List<SelectListItem> Categories { get; set; } = new();
        public List<SelectListItem> SubCategories { get; set; } = new();

        public int? SelectedCategoryId { get; set; }
        public int? SelectedSubCategoryId { get; set; }
        public int? SelectedMonth { get; set; }
        public int? SelectedYear { get; set; }

        public List<Student> Student { get; set; } = new List<Student>();
        public List<Attendance> AttendanceData { get; set; } = new();

        public int TotalDays { get; set; }

        public bool IsStudent { get; set; } = false;
    }
}
