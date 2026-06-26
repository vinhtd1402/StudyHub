using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using StudyHub.Models;
using StudyHub.Services;

namespace StudyHub.Pages_Courses
{
    public class IndexModel : PageModel
    {
        private readonly CourseService _courseService;

        public IndexModel(CourseService courseService)
        {
            _courseService = courseService;
        }

        public IList<Course> Course { get; set; } = default!;
        public SelectList TeacherOptions { get; set; } = default!;

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? TeacherId { get; set; }

        public async Task OnGetAsync()
        {
            Course = await _courseService.GetCoursesAsync(SearchTerm, TeacherId);

            var teachers = await _courseService.GetTeachersWithCoursesAsync();
            TeacherOptions = new SelectList(
                teachers.Select(t => new
                {
                    t.Id,
                    Name = string.IsNullOrWhiteSpace(t.FullName) ? t.Email : t.FullName
                }),
                "Id",
                "Name",
                TeacherId);
        }
    }
}
