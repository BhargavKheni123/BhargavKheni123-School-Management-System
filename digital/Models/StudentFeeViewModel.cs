namespace digital.ViewModels
{
    public class StudentFeeViewModel
    {
        public string StudentName { get; set; }
        public int Year { get; set; }
        public decimal TotalFees { get; set; }
        public decimal PaidFees { get; set; }
        public decimal RemainingFees => TotalFees - PaidFees;
    }
}
