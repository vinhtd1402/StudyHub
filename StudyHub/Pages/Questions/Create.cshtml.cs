using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StudyHub.Data;
using StudyHub.Models;

namespace StudyHub.Pages.Questions
{
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CreateModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public List<Question> Questions { get; set; }
            = new();

        [FromQuery]
        public int QuizId { get; set; }

        [FromQuery]
        public int Count { get; set; }

        public void OnGet()
        {
            for (int i = 0; i < Count; i++)
            {
                Questions.Add(new Question
                {
                    QuizId = QuizId
                });
            }
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Questions.AddRange(Questions);

            _context.SaveChanges();

            return RedirectToPage("/Quizzes/Index");
        }
    }
}