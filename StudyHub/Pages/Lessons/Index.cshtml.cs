using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
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

        public async Task OnGetAsync()
        {
            if (User.IsInRole("Teacher"))
            {
                Lessons = await _context.Lessons
                    .Include(l => l.Course)
                    .ToListAsync();
            }
            else
            {
                var userId = _userManager.GetUserId(User);

                Lessons = await _context.Lessons
                    .Include(l => l.Course)
                    .Where(l =>
                        _context.Enrollments.Any(e =>
                            e.StudentId == userId &&
                            e.CourseId == l.CourseId))
                    .ToListAsync();
            }
        }
    }
}