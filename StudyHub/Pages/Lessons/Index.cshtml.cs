using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudyHub.Data;
using StudyHub.Models;

namespace StudyHub.Pages_Lessons
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public IndexModel(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IList<Lesson> Lessons { get; set; } = default!;
        public SelectList CourseOptions { get; set; } = default!;

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? CourseId { get; set; }

        public async Task OnGetAsync()
        {
            var lessonsQuery = _context.Lessons
                .Include(l => l.Course)
                .AsQueryable();

            if (User.IsInRole("Admin"))
            {
                CourseOptions = await BuildCourseOptionsAsync(_context.Courses);
            }
            else if (User.IsInRole("Teacher"))
            {
                var userId = _userManager.GetUserId(User);

                lessonsQuery = lessonsQuery.Where(l =>
                    l.Course != null &&
                    l.Course.TeacherId == userId);

                CourseOptions = await BuildCourseOptionsAsync(
                    _context.Courses.Where(c => c.TeacherId == userId));
            }
            else
            {
                var userId = _userManager.GetUserId(User);

                lessonsQuery = lessonsQuery.Where(l =>
                    _context.Enrollments.Any(e =>
                        e.StudentId == userId &&
                        e.CourseId == l.CourseId));

                var enrolledCourseIds = _context.Enrollments
                    .Where(e => e.StudentId == userId)
                    .Select(e => e.CourseId);

                CourseOptions = await BuildCourseOptionsAsync(
                    _context.Courses.Where(c => enrolledCourseIds.Contains(c.Id)));
            }

            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                lessonsQuery = lessonsQuery.Where(l =>
                    l.Title.Contains(SearchTerm) ||
                    (l.Content != null && l.Content.Contains(SearchTerm)) ||
                    (l.Course != null && l.Course.Title.Contains(SearchTerm)));
            }

            if (CourseId.HasValue)
            {
                lessonsQuery = lessonsQuery.Where(l => l.CourseId == CourseId.Value);
            }

            Lessons = await lessonsQuery
                .OrderBy(l => l.Course != null ? l.Course.Title : string.Empty)
                .ThenBy(l => l.Id)
                .ToListAsync();
        }

        private async Task<SelectList> BuildCourseOptionsAsync(IQueryable<Course> coursesQuery)
        {
            var courses = await coursesQuery
                .OrderBy(c => c.Title)
                .Select(c => new { c.Id, c.Title })
                .ToListAsync();

            return new SelectList(courses, "Id", "Title", CourseId);
        }
    }
}
