using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StudyHub.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using StudyHub.Services;

namespace StudyHub.Pages.Quizzes
{
    [Authorize(Roles = "Teacher")]
    public class CreateModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AccessControlService _accessControlService;
        private readonly QuizService _quizService;

        public CreateModel(
            UserManager<ApplicationUser> userManager,
            AccessControlService accessControlService,
            QuizService quizService)
        {
            _userManager = userManager;
            _accessControlService = accessControlService;
            _quizService = quizService;
        }

        [BindProperty]
        public Quiz Quiz { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int lessonId)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Challenge();
            }

            if (!await _accessControlService.TeacherOwnsLessonAsync(user.Id, lessonId))
            {
                return Forbid();
            }

            Quiz.LessonId = lessonId;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Challenge();
            }

            if (!await _accessControlService.TeacherOwnsLessonAsync(user.Id, Quiz.LessonId))
            {
                return Forbid();
            }

            await _quizService.CreateQuizAsync(Quiz);

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
