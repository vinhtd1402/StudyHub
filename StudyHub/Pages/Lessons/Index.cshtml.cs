using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using StudyHub.Models;
using StudyHub.Services;

namespace StudyHub.Pages_Lessons
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly LessonService _lessonService;

        public IndexModel(
            UserManager<ApplicationUser> userManager,
            LessonService lessonService)
        {
            _userManager = userManager;
            _lessonService = lessonService;
        }

        public IList<Lesson> Lessons { get; set; } = default!;
        public SelectList CourseOptions { get; set; } = default!;

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? CourseId { get; set; }

        public async Task OnGetAsync()
        {
            var userId = _userManager.GetUserId(User);
            var isAdmin = User.IsInRole("Admin");
            var isTeacher = User.IsInRole("Teacher");

            Lessons = await _lessonService.GetLessonsAsync(
                userId,
                isAdmin,
                isTeacher,
                SearchTerm,
                CourseId);

            CourseOptions = await _lessonService.BuildCourseOptionsAsync(
                userId,
                isAdmin,
                isTeacher,
                CourseId);
        }
    }
}
