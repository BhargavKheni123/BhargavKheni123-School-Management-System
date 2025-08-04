using digital.Models;

public class Student
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }

    public int CategoryId { get; set; }
    public int SubCategoryId { get; set; }

    public DateTime DOB { get; set; }
    public string Gender { get; set; }
    public string MobileNumber { get; set; }
    public string Address { get; set; }

    public DateTime CreatedDate { get; set; }
    public Category Category { get; set; }
    public SubCategory SubCategory { get; set; }
}
