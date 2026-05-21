using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StudyHub.Data;
using StudyHub.Models;
using Microsoft.AspNetCore.Authorization;

namespace StudyHub.Pages.Quizzes
{
    [Authorize(Roles = "Teacher")]
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CreateModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Quiz Quiz { get; set; } = new();

        public IActionResult OnGet(int lessonId)
        {
            Quiz.LessonId = lessonId;
            return Page();
        }

        public IActionResult OnPost()
        {

            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Quizzes.Add(Quiz);
            _context.SaveChanges();

            return RedirectToPage(
                "/Questions/Create",
                new
                {
                    quizId = Quiz.Id,
                    count = Quiz.QuestionCount
                });
        }
    }
}