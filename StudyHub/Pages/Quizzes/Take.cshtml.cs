using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StudyHub.Models;
using StudyHub.Services;

namespace StudyHub.Pages.Quizzes
{
    [Authorize(Roles = "Student,Admin")]
    public class TakeModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly QuizService _quizService;

        public TakeModel(
            UserManager<ApplicationUser> userManager,
            QuizService quizService)
        {
            _userManager = userManager;
            _quizService = quizService;
        }

        public Quiz Quiz { get; set; } = default!;

        [BindProperty]
        public Dictionary<int, string> UserAnswers { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Challenge();
            }

            var quiz = await _quizService.GetQuizForTakingAsync(
                id,
                user.Id,
                User.IsInRole("Admin"));

            if (quiz == null)
            {
                return Forbid();
            }

            Quiz = quiz;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Challenge();
            }

            var quiz = await _quizService.GetQuizForTakingAsync(
                id,
                user.Id,
                User.IsInRole("Admin"));

            if (quiz == null)
            {
                return Forbid();
            }

            var attempt = await _quizService.SubmitAttemptAsync(id, user.Id, UserAnswers);

            return RedirectToPage("/Quizzes/Review", new { attemptId = attempt.Id });
        }
    }
}
