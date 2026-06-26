using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StudyHub.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using StudyHub.Services;

namespace StudyHub.Pages_Lessons
{
    [Authorize(Roles = "Teacher")]
    public class CreateModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AccessControlService _accessControlService;
        private readonly LessonService _lessonService;

        public CreateModel(
            UserManager<ApplicationUser> userManager,
            AccessControlService accessControlService,
            LessonService lessonService)
        {
            _userManager = userManager;
            _accessControlService = accessControlService;
            _lessonService = lessonService;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Challenge();
            }

            if (user.IsTeacherSuspended)
            {
                return Forbid();
            }

            await LoadCourseOptionsAsync(user.Id, null);
            return Page();
        }

        [BindProperty]
        public Lesson Lesson { get; set; } = default!;

        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser != null)
                {
                    await LoadCourseOptionsAsync(currentUser.Id, Lesson.CourseId);
                }

                return Page();
            }

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Challenge();
            }

            if (user.IsTeacherSuspended)
            {
                return Forbid();
            }

            if (!await _accessControlService.TeacherOwnsCourseAsync(user.Id, Lesson.CourseId))
            {
                return Forbid();
            }

            await _lessonService.CreateLessonAsync(Lesson);

            return RedirectToPage("./Index");
        }

        private async Task LoadCourseOptionsAsync(string teacherId, int? selectedCourseId)
        {
            ViewData["CourseId"] = await _lessonService.BuildCourseOptionsAsync(
                teacherId,
                isAdmin: false,
                isTeacher: true,
                selectedCourseId);
        }
    }
}
