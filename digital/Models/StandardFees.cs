namespace digital.Models
{
    public class StandardFees
    {

        public int Id { get; set; }
        public int CategoryId { get; set; }   
        public int Year { get; set; }
        public decimal TotalFees { get; set; }
        public DateTime CreatedDate { get; set; }

    }
}
