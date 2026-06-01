using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StudyHub.Models;
using Microsoft.AspNetCore.Identity;
using StudyHub.Services;

namespace StudyHub.Pages_Lessons
{
    [Authorize(Roles = "Teacher")]
    public class EditModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly LessonService _lessonService;

        public EditModel(
            UserManager<ApplicationUser> userManager,
            LessonService lessonService)
        {
            _userManager = userManager;
            _lessonService = lessonService;
        }

        [BindProperty]
        public Lesson Lesson { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
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

            var lesson = await _lessonService.GetTeacherLessonAsync(id.Value, user.Id);
            if (lesson == null)
            {
                return NotFound();
            }

            Lesson = lesson;
            await LoadCourseOptionsAsync(user.Id);
            return Page();
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser != null)
                {
                    await LoadCourseOptionsAsync(currentUser.Id);
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

            var result = await _lessonService.UpdateLessonAsync(Lesson, user.Id);
            if (result == null)
            {
                return NotFound();
            }

            if (result == false)
            {
                return Forbid();
            }

            return RedirectToPage("./Index");
        }

        private async Task LoadCourseOptionsAsync(string teacherId)
        {
            ViewData["CourseId"] = await _lessonService.BuildCourseOptionsAsync(
                teacherId,
                isAdmin: false,
                isTeacher: true,
                Lesson.CourseId);
        }
    }
}
