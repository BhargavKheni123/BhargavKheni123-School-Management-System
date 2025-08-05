using digital.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace digital.ViewModels
{
    public class TimeTableViewModel
    {
        public TimeTable TimeTable { get; set; } = new TimeTable
        {
            Std = "",
            Class = "",
            Subject = ""
        };

        public List<SelectListItem> StdList { get; set; } = new();
        public List<SelectListItem> ClassList { get; set; } = new();
        public List<SelectListItem> Hours { get; set; } = new();
        public List<SelectListItem> Minutes { get; set; } = new();

        public List<TimeTable> TimeTableList { get; set; } = new();
        public string Role { get; set; }
        public string Message { get; set; }
    }
}
