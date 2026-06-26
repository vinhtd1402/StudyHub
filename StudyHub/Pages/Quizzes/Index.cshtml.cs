using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StudyHub.Models;
using StudyHub.Services;

namespace StudyHub.Pages.Quizzes
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly QuizService _quizService;

        public IndexModel(
            UserManager<ApplicationUser> userManager,
            QuizService quizService)
        {
            _userManager = userManager;
            _quizService = quizService;
        }

        public List<Quiz> Quizzes { get; set; } = new();

        public async Task OnGetAsync()
        {
            var userId = _userManager.GetUserId(User);

            Quizzes = (await _quizService.GetVisibleQuizzesAsync(
                userId,
                User.IsInRole("Teacher"),
                User.IsInRole("Student"))).ToList();
        }
    }
}
