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

        public Quiz Quiz { get; set; } = default!;

        // QuestionId -> A/B/C/D

        [BindProperty]
        public Dictionary<int, string> UserAnswers { get; set; }
            = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Quiz = await _context.Quizzes
                .Include(q => q.Questions)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (Quiz == null)
            {
                return NotFound();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var quiz = await _context.Quizzes
                .Include(q => q.Questions)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (quiz == null)
            {
                return NotFound();
            }

            int score = 0;

            foreach (var question in quiz.Questions)
            {
                if (UserAnswers.TryGetValue(
                    question.Id,
                    out string? selectedAnswer))
                {
                    if (selectedAnswer == question.CorrectAnswer)
                    {
                        score++;
                    }
                }
            }
            var user = await _context.Users
    .FirstOrDefaultAsync(u => u.UserName == User.Identity!.Name);

            var attempt = new QuizAttempt
            {
                QuizId = quiz.Id,
                UserId = user!.Id,
                Score = score,
                TotalQuestions = quiz.Questions.Count,
                TakenAt = DateTime.Now
            };

            _context.QuizAttempts.Add(attempt);
            await _context.SaveChangesAsync();

            ViewData["Score"] = score;
            ViewData["Total"] = quiz.Questions.Count;

            Quiz = quiz;

            // 🔥 QUAN TRỌNG NHẤT
            IsSubmitted = true;

            return Page();
        }
        public bool IsSubmitted { get; set; } = false;

        
    }
}