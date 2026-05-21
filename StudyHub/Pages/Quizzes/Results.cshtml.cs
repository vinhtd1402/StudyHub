using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using StudyHub.Data;
using StudyHub.Models;

namespace StudyHub.Pages.Quizzes
{
    public class ResultsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public ResultsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public Quiz Quiz { get; set; } = default!;

        public List<QuizAttempt> Attempts { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int quizId)
        {
            Quiz = await _context.Quizzes
                .FirstOrDefaultAsync(q => q.Id == quizId);

            if (Quiz == null)
            {
                return NotFound();
            }

            Attempts = await _context.QuizAttempts
                .Include(a => a.User)
                .Where(a => a.QuizId == quizId)
                .OrderByDescending(a => a.TakenAt)
                .ToListAsync();

            return Page();
        }
    }
}