namespace digital.Models
{
    public class TimeTableViewModel
    {
        public int Id { get; set; }
        public string StdName { get; set; }
        public string ClassName { get; set; }

        public string Subject { get; set; }
        public string TeacherName { get; set; }

        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }

        public string Day { get; set; }
    }

}
