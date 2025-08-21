namespace digital.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Navigation
        public ICollection<SubCategory> SubCategories { get; set; }
    }
}