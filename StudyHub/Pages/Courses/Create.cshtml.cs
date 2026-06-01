using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StudyHub.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using StudyHub.Services;

namespace StudyHub.Pages_Courses
{
    [Authorize(Roles = "Teacher")]
    public class CreateModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly CourseService _courseService;

        public CreateModel(
            UserManager<ApplicationUser> userManager,
            CourseService courseService)
        {
            _userManager = userManager;
            _courseService = courseService;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        [BindProperty]
        public Course Course { get; set; } = default!;

        // For more information, see https://aka.ms/RazorPagesCRUD.
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

            if (user.IsTeacherSuspended)
            {
                return Forbid();
            }

            await _courseService.CreateCourseAsync(Course, user);

            return RedirectToPage("./Index");
        }
    }
}
