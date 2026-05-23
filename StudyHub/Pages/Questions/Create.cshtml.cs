using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StudyHub.Data;
using StudyHub.Models;
using StudyHub.Services;

namespace StudyHub.Pages.Questions
{
    [Authorize(Roles = "Teacher")]
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly QuizService _quizService;

        public CreateModel(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            QuizService quizService)
        {
            _context = context;
            _userManager = userManager;
            _quizService = quizService;
        }

        [BindProperty]
        public List<Question> Questions { get; set; }
            = new();

        [BindProperty(SupportsGet = true)]
        public int QuizId { get; set; }

        [BindProperty(SupportsGet = true)]
        public int Count { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Challenge();
            }

            if (!await _quizService.TeacherOwnsQuizAsync(user.Id, QuizId))
            {
                return Forbid();
            }

            for (int i = 0; i < Count; i++)
            {
                Questions.Add(new Question
                {
                    QuizId = QuizId
                });
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Count = Questions.Count;

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Challenge();
            }

            if (Questions.Any(q => q.QuizId != QuizId) ||
                !await _quizService.TeacherOwnsQuizAsync(user.Id, QuizId))
            {
                return Forbid();
            }

            _context.Questions.AddRange(Questions);
            await _context.SaveChangesAsync();

            return RedirectToPage("/Quizzes/Index");
        }
    }
}
