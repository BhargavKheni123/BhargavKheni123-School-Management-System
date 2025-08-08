using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace digital.Models
{
    public class Attendance
    {
        public int Id { get; set; }

        [ForeignKey("Student")]
        public int StudentId { get; set; }

        public int Day { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }

        [DataType(DataType.Date)]
        public DateTime FullDate { get; set; }

        public string Attend { get; set; }

        public Student Student { get; set; }
    
    }
}