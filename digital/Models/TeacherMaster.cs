  using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    namespace digital.Models
    {
        [Table("TeacherMaster")]
        public class TeacherMaster
        {
            public int Id { get; set; }

            [Required(ErrorMessage = "Teacher is required")]
            public int TeacherId { get; set; }

            [Required(ErrorMessage = "Category is required")]
            public int CategoryId { get; set; }

            [Required(ErrorMessage = "SubCategory is required")]
            public int SubCategoryId { get; set; }

            [Required(ErrorMessage = "Subject is required")]
            public int SubjectId { get; set; }

            public DateTime CreatedDate { get; set; }

            public User Teacher { get; set; }
            public Category Category { get; set; }
            public SubCategory SubCategory { get; set; }
            public Subject Subject { get; set; }
        }
    }
