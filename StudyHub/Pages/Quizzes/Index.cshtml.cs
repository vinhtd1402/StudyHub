using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using StudyHub.Data;
using StudyHub.Models;

namespace StudyHub.Pages.Quizzes
{
    [Authorize]
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

        public List<Quiz> Quizzes { get; set; } = new();

        public async Task OnGetAsync()
        {
            var userId = _userManager.GetUserId(User);

            var query = _context.Quizzes
                .Include(q => q.Lesson)
                .ThenInclude(l => l!.Course)
                .AsQueryable();

            if (User.IsInRole("Teacher"))
            {
                query = query.Where(q =>
                    q.Lesson != null &&
                    q.Lesson.Course != null &&
                    q.Lesson.Course.TeacherId == userId);
            }
            else if (User.IsInRole("Student"))
            {
                query = query.Where(q =>
                    q.Lesson != null &&
                    _context.Enrollments.Any(e =>
                        e.StudentId == userId &&
                        e.CourseId == q.Lesson.CourseId));
            }

            Quizzes = await query
                .OrderBy(q => q.Lesson != null ? q.Lesson.Title : string.Empty)
                .ThenBy(q => q.Title)
                .ToListAsync();
        }
    }
}
