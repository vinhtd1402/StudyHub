using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StudyHub.Data;
using StudyHub.Models;

namespace StudyHub.Pages.Quizzes
{
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CreateModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Quiz Quiz { get; set; } = new();

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Quizzes.Add(Quiz);
            await _context.SaveChangesAsync();

            // ⚠ FIX QUAN TRỌNG
            return RedirectToPage("/Quizzes/Index");
        }
    }
}