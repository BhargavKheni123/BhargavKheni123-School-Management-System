namespace digital.Models
{
    public class StudentFees
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public int Year { get; set; }
        public decimal PaidAmount { get; set; }
        public DateTime? PaidDate { get; set; }
    }
}
