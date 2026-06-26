using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StudyHub.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using StudyHub.Services;

namespace StudyHub.Pages_Lessons
{
    [Authorize(Roles = "Teacher")]
    public class DeleteModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly LessonService _lessonService;

        public DeleteModel(
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

            var lesson = await _lessonService.GetTeacherLessonAsync(id.Value, user.Id);
            if (lesson == null)
            {
                return NotFound();
            }

            Lesson = lesson;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
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

            var result = await _lessonService.DeleteLessonAsync(id.Value, user.Id);
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
    }
}
