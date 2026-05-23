using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StudyHub.Data;
using StudyHub.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using StudyHub.Services;

namespace StudyHub.Pages.Quizzes
{
    [Authorize(Roles = "Teacher")]
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AccessControlService _accessControlService;

        public CreateModel(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            AccessControlService accessControlService)
        {
            _context = context;
            _userManager = userManager;
            _accessControlService = accessControlService;
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

            _context.Quizzes.Add(Quiz);
            await _context.SaveChangesAsync();

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
