using digital.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace digital.ViewModels
{
    public class TimeTableViewModel
    {
        public TimeTableViewModel()
        {
            TimeTable = new TimeTable
            {
                Std = string.Empty,
                Class = string.Empty,
                Subject = string.Empty
            };
        }

        public TimeTable TimeTable { get; set; }

        public List<SelectListItem> StdList { get; set; } = new();
        public List<SelectListItem> ClassList { get; set; } = new();
        public List<SelectListItem> Hours { get; set; } = new();
        public List<SelectListItem> Minutes { get; set; } = new();
        public List<TimeTable> TimeTableList { get; set; } = new();

        public string Role { get; set; }
        public string Message { get; set; }
    }
}
