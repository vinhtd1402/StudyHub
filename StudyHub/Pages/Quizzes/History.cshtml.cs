using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using StudyHub.Data;
using StudyHub.Models;

namespace StudyHub.Pages.Quizzes
{
    public class HistoryModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public HistoryModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<QuizAttempt> History { get; set; } = new();

        public void OnGet()
        {
            string userId = User.Identity?.Name ?? "Anonymous";

            History = _context.QuizAttempts
                .Include(x => x.Quiz)
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.TakenAt)
                .ToList();
        }
    }
}