using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StudyHub.Models;
using StudyHub.Services;

namespace StudyHub.Pages_Courses
{
    [Authorize(Roles = "Student,Teacher")]
    public class MyCoursesModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly CourseService _courseService;

        public Dictionary<int, int> CompletedLessonsCount { get; set; } = new();
        public Dictionary<int, int> TotalLessonsCount { get; set; } = new();

        public MyCoursesModel(
            UserManager<ApplicationUser> userManager,
            CourseService courseService)
        {
            _userManager = userManager;
            _courseService = courseService;
        }

        public IList<Course> Courses { get; set; } = new List<Course>();

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public string StatusFilter { get; set; } = "all";

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null) return;

            var data = await _courseService.GetMyCoursesAsync(
                user.Id,
                User.IsInRole("Student"),
                User.IsInRole("Teacher"),
                SearchTerm,
                StatusFilter);

            Courses = data.Courses;
            CompletedLessonsCount = data.CompletedLessonsCount;
            TotalLessonsCount = data.TotalLessonsCount;
        }

        public async Task<IActionResult> OnGetContinueAsync(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null) return Challenge();

            var nextLesson = await _courseService.GetNextLessonToContinueAsync(id, user.Id);
            if (nextLesson != null)
            {
                return RedirectToPage("/Lessons/Details", new { id = nextLesson.Id });
            }

            return RedirectToPage();
        }

    }
}
