using digital.Models;
using Microsoft.EntityFrameworkCore;
using System.Xml;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    { }
    public DbSet<User> Users { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<SubCategory> SubCategories { get; set; }
    // add other tables here
    public DbSet<TimeTable> TimeTables { get; set; }
    public DbSet<Student> Student { get; set; }
    public DbSet<Attendance> Attendance { get; set; }
    public DbSet<Subject> Subjects { get; set; }
    public DbSet<TeacherMaster> TeacherMaster { get; set; }

}


