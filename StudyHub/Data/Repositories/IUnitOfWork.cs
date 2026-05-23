using StudyHub.Models;

namespace StudyHub.Data.Repositories
{
    public interface IUnitOfWork
    {
        IRepository<ApplicationUser> Users { get; }
        IRepository<Course> Courses { get; }
        IRepository<Lesson> Lessons { get; }
        IRepository<Enrollment> Enrollments { get; }
        IRepository<LessonProgress> LessonProgresses { get; }
        IRepository<CreditTransaction> CreditTransactions { get; }
        IRepository<Quiz> Quizzes { get; }
        IRepository<Question> Questions { get; }
        IRepository<QuizAttempt> QuizAttempts { get; }
        IRepository<QuizAttemptAnswer> QuizAttemptAnswers { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
