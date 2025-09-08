using digital.Models;
using digital.Models;
using Digital.Models;
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
    public DbSet<TimeTable> TimeTables { get; set; }
    public DbSet<Student> Student { get; set; }
    public DbSet<Attendance> Attendance { get; set; }
    public DbSet<Subject> Subjects { get; set; }
    public DbSet<TeacherMaster> TeacherMaster { get; set; }

    public DbSet<Teacher> Teachers { get; set; }

    public DbSet<Exam> Exams { get; set; }
    public DbSet<QuestionMaster> QuestionMaster { get; set; }
    public DbSet<AnswerOptions> AnswerOptions { get; set; }
    public DbSet<StudentAnswer> StudentAnswers { get; set; }

    public DbSet<StudentExamResult> StudentExamResults { get; set; }

    public DbSet<StandardFees> StandardFees { get; set; }
    public DbSet<StudentFees> StudentFees { get; set; }

    public DbSet<Assignment> Assignment { get; set; }
    public DbSet<AssignmentSubmission> AssignmentSubmissions { get; set; }




    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AnswerOptions>()
            .HasOne(a => a.Question)
            .WithMany(q => q.AnswerOptions)
            .HasForeignKey(a => a.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<AssignmentSubmission>()
           .HasOne(s => s.Assignment)
           .WithMany(a => a.Submission)
           .HasForeignKey(s => s.AssignmentId);

        modelBuilder.Entity<AssignmentSubmission>()
            .HasOne(s => s.Student)
            .WithMany()
            .HasForeignKey(s => s.StudentId);
    }


}
