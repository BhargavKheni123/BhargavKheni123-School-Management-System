using digital.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace digital.Models
{
    
    public class SubCategoryViewModel
    {

        public int Id { get; set; }
        public int CategoryId { get; set; }
        public string Name { get; set; }

        public List<SelectListItem> Categories { get; set; }
        public List<SubCategory> SubCategoryList { get; set; }

        public SubCategory SubCategoryToEdit { get; set; } 
    }


}