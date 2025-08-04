namespace digital.Models
{
    public class AttendanceViewModel
    {
        public int StudentId { get; set; }
        public int Day { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }

        public string Attend { get; set; }
        public DateTime FullDate { get; set; }
    }

}
