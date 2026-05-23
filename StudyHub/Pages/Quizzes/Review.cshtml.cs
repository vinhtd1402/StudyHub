using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StudyHub.Models;
using StudyHub.Services;

namespace StudyHub.Pages.Quizzes
{
    [Authorize]
    public class ReviewModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly QuizService _quizService;

        public ReviewModel(
            UserManager<ApplicationUser> userManager,
            QuizService quizService)
        {
            _userManager = userManager;
            _quizService = quizService;
        }

        public QuizAttempt Attempt { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int attemptId)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Challenge();
            }

            var attempt = await _quizService.GetAttemptReviewAsync(attemptId);

            if (attempt == null)
            {
                return NotFound();
            }

            var canReview =
                User.IsInRole("Admin") ||
                attempt.UserId == user.Id ||
                (User.IsInRole("Teacher") &&
                    await _quizService.TeacherOwnsQuizAsync(user.Id, attempt.QuizId));

            if (!canReview)
            {
                return Forbid();
            }

            Attempt = attempt;
            return Page();
        }
    }
}
