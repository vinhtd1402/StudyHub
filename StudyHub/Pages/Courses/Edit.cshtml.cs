using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StudyHub.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using StudyHub.Services;

namespace StudyHub.Pages_Courses
{
    [Authorize(Roles = "Teacher")]
    public class EditModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly CourseService _courseService;

        public EditModel(
            UserManager<ApplicationUser> userManager,
            CourseService courseService)
        {
            _userManager = userManager;
            _courseService = courseService;
        }

        [BindProperty]
        public Course Course { get; set; } = default!;

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

            var course = await _courseService.GetTeacherCourseAsync(id.Value, user.Id);
            if (course == null)
            {
                return NotFound();
            }

            Course = course;
            return Page();
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
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

            var result = await _courseService.UpdateCourseAsync(Course, user.Id);
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
