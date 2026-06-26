using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StudyHub.Models;
using StudyHub.Services;

namespace StudyHub.Pages_Lessons
{
    public class DetailsModel : PageModel
    {
        public string? EmbedYoutubeUrl { get; set; }
        public Lesson? PreviousLesson { get; set; }
        public Lesson? NextLesson { get; set; }
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly LessonService _lessonService;

        public DetailsModel(
            UserManager<ApplicationUser> userManager,
            LessonService lessonService)
        {
            _userManager = userManager;
            _lessonService = lessonService;
        }

        public Lesson Lesson { get; set; } = default!;
        public List<QuizResultViewModel> QuizResults { get; set; } = new();
        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);
            var (result, data) = await _lessonService.GetLessonDetailsAsync(
                id.Value,
                userId,
                User.IsInRole("Admin"),
                User.IsInRole("Teacher"));

            if (result == LessonAccessResult.NotFound)
            {
                return NotFound();
            }

            if (result == LessonAccessResult.Forbidden)
            {
                return Forbid();
            }

            if (result == LessonAccessResult.EnrollmentRequired)
            {
                TempData["Error"] = "You must enroll in this course first.";
                return RedirectToPage("/Courses/Details", new { id = data?.Lesson.CourseId ?? 0 });
            }

            Lesson = data!.Lesson;
            PreviousLesson = data.PreviousLesson;
            NextLesson = data.NextLesson;
            QuizResults = data.QuizResults;

            return Page();
        }

        public async Task<IActionResult> OnPostCompleteAsync(int? id)
        {
            if (id == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var completed = await _lessonService.MarkCompletedAsync(id.Value, user.Id);
            if (!completed)
            {
                return Forbid();
            }

            return RedirectToPage(new { id });
        }
    }
}
