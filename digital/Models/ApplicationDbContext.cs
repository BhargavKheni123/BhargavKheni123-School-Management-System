using digital.Models;
using Microsoft.EntityFrameworkCore;
using System.Xml;
using digital.Models;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    { }

    public DbSet<User> Users { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<SubCategory> SubCategories { get; set; }
    public DbSet<TimeTable> TimeTables { get; set; }
    public DbSet<Student> Student { get; set; }
    public DbSet<Attendance> Attendance { get; set; }
    public DbSet<Subject> Subjects { get; set; }
    public DbSet<TeacherMaster> TeacherMaster { get; set; }

    
    public DbSet<Exam> Exams { get; set; }
    public DbSet<ExamTeacher> ExamTeachers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        
        modelBuilder.Entity<Exam>()
            .HasMany(e => e.ExamTeachers)
            .WithOne(et => et.Exam)
            .HasForeignKey(et => et.ExamId);

        modelBuilder.Entity<ExamTeacher>()
            .HasOne(et => et.Teacher)
            .WithMany() 
            .HasForeignKey(et => et.TeacherId);
    }
}
