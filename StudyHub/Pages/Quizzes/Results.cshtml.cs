using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StudyHub.Models;
using StudyHub.Services;

namespace StudyHub.Pages.Quizzes
{
    [Authorize(Roles = "Teacher,Admin")]
    public class ResultsModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly QuizService _quizService;

        public ResultsModel(
            UserManager<ApplicationUser> userManager,
            QuizService quizService)
        {
            _userManager = userManager;
            _quizService = quizService;
        }

        public Quiz Quiz { get; set; } = default!;
        public IList<QuizAttempt> Attempts { get; set; } = new List<QuizAttempt>();

        public async Task<IActionResult> OnGetAsync(int quizId)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Challenge();
            }

            var quiz = await _quizService.GetQuizAsync(quizId);

            if (quiz == null)
            {
                return NotFound();
            }

            if (User.IsInRole("Teacher") &&
                !await _quizService.TeacherOwnsQuizAsync(user.Id, quizId))
            {
                return Forbid();
            }

            Quiz = quiz;
            Attempts = await _quizService.GetQuizResultsAsync(quizId);

            return Page();
        }
    }
}
