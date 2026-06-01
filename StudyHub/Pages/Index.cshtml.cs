using Microsoft.AspNetCore.Mvc.RazorPages;
using StudyHub.Models;
using StudyHub.Services;

namespace StudyHub.Pages
{
    public class IndexModel : PageModel
    {
        private readonly CourseService _courseService;

        public IndexModel(CourseService courseService)
        {
            _courseService = courseService;
        }

        public IList<Course> PopularCourses { get; set; }
            = new List<Course>();

        public async Task OnGetAsync()
        {
            PopularCourses = await _courseService.GetPopularCoursesAsync();
        }
    }
}
