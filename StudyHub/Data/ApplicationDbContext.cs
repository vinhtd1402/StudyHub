using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StudyHub.Models;

namespace StudyHub.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Quiz> Quizzes { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<QuizAttempt> QuizAttempts { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Lesson> Lessons { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<LessonProgress> LessonProgresses { get; set; }
        public DbSet<CreditTransaction> CreditTransactions { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Enrollment>()
                .HasOne(e => e.Course)
                .WithMany(c => c.Enrollments)
                .HasForeignKey(e => e.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Enrollment>()
                .HasOne(e => e.Student)
                .WithMany()
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Course>()
                .HasOne(c => c.Teacher)
                .WithMany()
                .HasForeignKey(c => c.TeacherId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<QuizAttempt>()
                .HasOne(q => q.User)
                .WithMany()
                .HasForeignKey(q => q.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ApplicationUser>()
                .Property(u => u.WalletBalance)
                .HasColumnType("decimal(18,0)");

            builder.Entity<ApplicationUser>()
                .Property(u => u.TeacherSuspensionReason)
                .HasMaxLength(500);

            builder.Entity<Course>()
                .Property(c => c.Price)
                .HasColumnType("decimal(18,0)");

            builder.Entity<Enrollment>()
                .Property(e => e.PricePaid)
                .HasColumnType("decimal(18,0)");

            builder.Entity<CreditTransaction>()
                .HasIndex(t => t.OrderId)
                .IsUnique();

            builder.Entity<CreditTransaction>()
                .HasIndex(t => t.RequestId)
                .IsUnique();

            builder.Entity<CreditTransaction>()
                .HasOne(t => t.User)
                .WithMany()
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
