using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using StudyHub.Models;
using System.Security.Claims;
using StudyHub.Services;

namespace StudyHub.Pages_Courses
{
    public class DetailsModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly CourseService _courseService;

        public IList<ApplicationUser> EnrolledStudents { get; set; } = new List<ApplicationUser>();
        public bool IsEnrolled { get; set; }
        public string? StatusMessage { get; set; }
        public Course Course { get; set; } = default!;

        public DetailsModel(
            UserManager<ApplicationUser> userManager,
            CourseService courseService)
        {
            _userManager = userManager;
            _courseService = courseService;
        }


        public async Task<IActionResult> OnGetAsync(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var details = await _courseService.GetCourseDetailsAsync(id, userId);
            if (details == null)
            {
                return NotFound();
            }

            Course = details.Course;
            IsEnrolled = details.IsEnrolled;
            EnrolledStudents = details.EnrolledStudents;

            return Page();
        }

        public async Task<IActionResult> OnPostEnrollAsync(int? id)
        {
            if (!User.IsInRole("Student")) // 🔥 THÊM
            {
                return Forbid();
            }

            if (id == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);

            if (user == null) return Challenge();

            var result = await _courseService.EnrollAsync(id.Value, user);
            if (result == CourseEnrollmentResult.CourseNotFound)
            {
                return NotFound();
            }

            if (result == CourseEnrollmentResult.InsufficientBalance)
            {
                StatusMessage = "Your StudyHub wallet does not have enough balance to enroll in this course.";
                await OnGetAsync(id.Value);
                return Page();
            }

            return RedirectToPage(new { id });
        }
        public async Task<IActionResult> OnPostUnenrollAsync(int id)
        {
            if (!User.IsInRole("Student")) // 🔥 THÊM
            {
                return Forbid();
            }

            var user = await _userManager.GetUserAsync(User);

            if (user == null) return Challenge();

            await _courseService.UnenrollAsync(id, user.Id);

            return RedirectToPage(new { id });
        }
    }
}
