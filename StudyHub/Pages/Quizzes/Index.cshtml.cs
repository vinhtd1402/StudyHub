using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using StudyHub.Data;
using StudyHub.Models;

namespace StudyHub.Pages.Quizzes
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Quiz> Quizzes { get; set; } = new();

        public void OnGet()
        {
            Quizzes = _context.Quizzes
                .Include(q => q.Lesson)
                .ToList();
        }
    }
}