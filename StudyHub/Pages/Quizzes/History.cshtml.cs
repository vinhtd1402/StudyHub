using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StudyHub.Models;
using StudyHub.Services;

namespace StudyHub.Pages.Quizzes
{
    [Authorize(Roles = "Student,Admin")]
    public class HistoryModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly QuizService _quizService;

        public HistoryModel(
            UserManager<ApplicationUser> userManager,
            QuizService quizService)
        {
            _userManager = userManager;
            _quizService = quizService;
        }

        public IList<QuizAttempt> History { get; set; } = new List<QuizAttempt>();

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Challenge();
            }

            History = await _quizService.GetStudentHistoryAsync(user.Id);
            return Page();
        }
    }
}
