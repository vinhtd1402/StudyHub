using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StudyHub.Models;
using StudyHub.Services;

namespace StudyHub.Pages.Courses
{
    public class CertificateModel : PageModel
    {
        private readonly CourseService _courseService;

        public CertificateModel(CourseService courseService)
        {
            _courseService = courseService;
        }

        public Course Course { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var course = await _courseService.GetCourseAsync(id);

            if (course == null)
            {
                return NotFound();
            }

            Course = course;
            return Page();
        }
    }
}
