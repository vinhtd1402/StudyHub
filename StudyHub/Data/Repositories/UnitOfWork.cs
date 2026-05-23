using StudyHub.Models;

namespace StudyHub.Data.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
            Users = new EfRepository<ApplicationUser>(context);
            Courses = new EfRepository<Course>(context);
            Lessons = new EfRepository<Lesson>(context);
            Enrollments = new EfRepository<Enrollment>(context);
            LessonProgresses = new EfRepository<LessonProgress>(context);
            CreditTransactions = new EfRepository<CreditTransaction>(context);
            Quizzes = new EfRepository<Quiz>(context);
            Questions = new EfRepository<Question>(context);
            QuizAttempts = new EfRepository<QuizAttempt>(context);
            QuizAttemptAnswers = new EfRepository<QuizAttemptAnswer>(context);
        }

        public IRepository<ApplicationUser> Users { get; }
        public IRepository<Course> Courses { get; }
        public IRepository<Lesson> Lessons { get; }
        public IRepository<Enrollment> Enrollments { get; }
        public IRepository<LessonProgress> LessonProgresses { get; }
        public IRepository<CreditTransaction> CreditTransactions { get; }
        public IRepository<Quiz> Quizzes { get; }
        public IRepository<Question> Questions { get; }
        public IRepository<QuizAttempt> QuizAttempts { get; }
        public IRepository<QuizAttemptAnswer> QuizAttemptAnswers { get; }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
