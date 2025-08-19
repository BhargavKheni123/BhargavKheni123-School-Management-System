using Microsoft.AspNetCore.Mvc.Rendering;

namespace digital.Models
{
    public class QuestionMasterViewModel
    {
        public int CategoryId { get; set; }
        public int SubjectId { get; set; }
        public string ExamType { get; set; }
        public string QuestionText { get; set; }

        public string Answer1 { get; set; }
        public string Answer2 { get; set; }
        public string Answer3 { get; set; }
        public string Answer4 { get; set; }

        public string RightAnswer { get; set; }

        public List<SelectListItem> Categories { get; set; }
        public List<SelectListItem> Subjects { get; set; }

    }

}
