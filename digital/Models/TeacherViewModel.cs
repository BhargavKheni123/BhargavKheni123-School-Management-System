using System;
using System.ComponentModel.DataAnnotations;

namespace digital.ViewModels
{
    public class TeacherViewModel
    {
        [Required]
        public string Name { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required, DataType(DataType.Password)]
        public string Password { get; set; }

        [Required, DataType(DataType.Date)]
        public DateTime DOB { get; set; }

        [Required]
        public string Gender { get; set; }

        public string Address { get; set; }
    }
}
