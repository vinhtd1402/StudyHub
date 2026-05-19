using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using StudyHub.Data;
using StudyHub.Models;

namespace StudyHub.Pages.Quizzes
{
    public class TakeModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public TakeModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public Quiz Quiz { get; set; }

        // Lưu đáp án user chọn: QuestionId -> AnswerId
        [BindProperty]
        public Dictionary<int, int> UserAnswers { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Quiz = await _context.Quizzes
                .Include(q => q.Questions)
                    .ThenInclude(q => q.Answers)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (Quiz == null)
                return NotFound();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var quiz = await _context.Quizzes
                .Include(q => q.Questions)
                    .ThenInclude(q => q.Answers)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (quiz == null)
                return NotFound();

            int score = 0;

            foreach (var question in quiz.Questions)
            {
                if (UserAnswers.TryGetValue(question.Id, out int answerId))
                {
                    var answer = question.Answers.FirstOrDefault(a => a.Id == answerId);

                    if (answer != null && answer.IsCorrect)
                        score++;
                }
            }

            // 🔥 SAVE RESULT
            var attempt = new QuizAttempt
            {
                QuizId = quiz.Id,
                UserId = User.Identity?.Name ?? "Anonymous",
                Score = score,
                TotalQuestions = quiz.Questions.Count,
                TakenAt = DateTime.Now
            };

            _context.QuizAttempts.Add(attempt);
            await _context.SaveChangesAsync();

            ViewData["Score"] = score;
            ViewData["Total"] = quiz.Questions.Count;

            Quiz = quiz;
            return Page();
        }
    }
}