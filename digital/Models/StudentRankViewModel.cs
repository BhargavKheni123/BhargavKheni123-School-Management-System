using digital.Models;
using System.Collections.Generic;

namespace digital.ViewModels
{
    public class StudentRankViewModel
    {
        public int CategoryId { get; set; }  
        public int SubCategoryId { get; set; }
        public int SubjectId { get; set; }    

        
        public List<Category> Categories { get; set; }
        public List<SubCategory> SubCategories { get; set; }
        public List<Subject> Subjects { get; set; }

        
        public List<StudentRankData> StudentRanks { get; set; }
    }

    public class StudentRankData
    {
        public string StudentName { get; set; }
        public int TotalMarks { get; set; }
        public int Rank { get; set; }
    }
}
