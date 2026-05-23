using Microsoft.EntityFrameworkCore;
using StudyHub.Data.Repositories;
using StudyHub.Models;

namespace StudyHub.Services
{
    public class QuizService
    {
        private readonly IUnitOfWork _unitOfWork;

        public QuizService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Quiz?> GetQuizForTakingAsync(int quizId, string userId, bool isAdmin)
        {
            var quiz = await _unitOfWork.Quizzes.Query()
                .Include(q => q.Lesson)
                .Include(q => q.Questions)
                .FirstOrDefaultAsync(q => q.Id == quizId);

            if (quiz == null)
            {
                return null;
            }

            if (isAdmin || await CanStudentTakeQuizAsync(userId, quizId))
            {
                return quiz;
            }

            return null;
        }

        public async Task<bool> CanStudentTakeQuizAsync(string userId, int quizId)
        {
            var quiz = await _unitOfWork.Quizzes.Query()
                .Include(q => q.Lesson)
                .FirstOrDefaultAsync(q => q.Id == quizId);

            if (quiz?.Lesson == null)
            {
                return false;
            }

            return await _unitOfWork.Enrollments.AnyAsync(e =>
                e.StudentId == userId &&
                e.CourseId == quiz.Lesson.CourseId);
        }

        public async Task<bool> TeacherOwnsQuizAsync(string teacherId, int quizId)
        {
            return await _unitOfWork.Quizzes.Query()
                .Include(q => q.Lesson)
                .ThenInclude(l => l!.Course)
                .AnyAsync(q =>
                    q.Id == quizId &&
                    q.Lesson != null &&
                    q.Lesson.Course != null &&
                    q.Lesson.Course.TeacherId == teacherId);
        }

        public async Task<QuizAttempt> SubmitAttemptAsync(
            int quizId,
            string userId,
            IDictionary<int, string> userAnswers)
        {
            var quiz = await _unitOfWork.Quizzes.Query()
                .Include(q => q.Questions)
                .FirstOrDefaultAsync(q => q.Id == quizId);

            if (quiz == null)
            {
                throw new InvalidOperationException("Quiz not found.");
            }

            var normalizedAnswers = userAnswers
                .Where(item => item.Value is "A" or "B" or "C" or "D")
                .ToDictionary(item => item.Key, item => item.Value);

            var answers = new List<QuizAttemptAnswer>();
            var score = 0;

            foreach (var question in quiz.Questions.OrderBy(q => q.Id))
            {
                normalizedAnswers.TryGetValue(question.Id, out var selectedAnswer);
                var isCorrect = selectedAnswer == question.CorrectAnswer;

                if (isCorrect)
                {
                    score++;
                }

                answers.Add(new QuizAttemptAnswer
                {
                    QuestionId = question.Id,
                    QuestionContent = question.Content,
                    OptionA = question.OptionA,
                    OptionB = question.OptionB,
                    OptionC = question.OptionC,
                    OptionD = question.OptionD,
                    SelectedAnswer = selectedAnswer,
                    CorrectAnswer = question.CorrectAnswer,
                    IsCorrect = isCorrect
                });
            }

            var totalQuestions = answers.Count;
            var percentage = totalQuestions == 0
                ? 0m
                : Math.Round(score * 100m / totalQuestions, 2);

            var attempt = new QuizAttempt
            {
                QuizId = quiz.Id,
                UserId = userId,
                Score = score,
                TotalQuestions = totalQuestions,
                Percentage = percentage,
                IsPassed = percentage >= quiz.PassingScorePercent,
                TakenAt = DateTime.UtcNow,
                Answers = answers
            };

            await _unitOfWork.QuizAttempts.AddAsync(attempt);
            await _unitOfWork.SaveChangesAsync();

            return attempt;
        }

        public async Task<IList<QuizAttempt>> GetStudentHistoryAsync(string userId)
        {
            return await _unitOfWork.QuizAttempts.Query()
                .Include(a => a.Quiz)
                .ThenInclude(q => q.Lesson)
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.TakenAt)
                .ToListAsync();
        }

        public async Task<IList<QuizAttempt>> GetQuizResultsAsync(int quizId)
        {
            return await _unitOfWork.QuizAttempts.Query()
                .Include(a => a.User)
                .Where(a => a.QuizId == quizId)
                .OrderByDescending(a => a.TakenAt)
                .ToListAsync();
        }

        public async Task<Quiz?> GetQuizAsync(int quizId)
        {
            return await _unitOfWork.Quizzes.Query()
                .Include(q => q.Lesson)
                .FirstOrDefaultAsync(q => q.Id == quizId);
        }

        public async Task<QuizAttempt?> GetAttemptReviewAsync(int attemptId)
        {
            return await _unitOfWork.QuizAttempts.Query()
                .Include(a => a.User)
                .Include(a => a.Quiz)
                .ThenInclude(q => q.Lesson)
                .Include(a => a.Answers)
                .FirstOrDefaultAsync(a => a.Id == attemptId);
        }
    }
}
