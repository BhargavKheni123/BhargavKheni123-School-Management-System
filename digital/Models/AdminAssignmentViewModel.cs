using Microsoft.AspNetCore.Mvc.Rendering;

namespace digital.Models
{
    public class AdminAssignmentViewModel
    {
        public Assignment Assignment { get; set; }

        public List<SelectListItem> Teachers { get; set; }
        public List<SelectListItem> Categories { get; set; }
        public List<SelectListItem> SubCategories { get; set; }
        public List<SelectListItem> Subjects { get; set; }
    }

}
