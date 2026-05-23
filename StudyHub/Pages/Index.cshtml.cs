using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using StudyHub.Data;
using StudyHub.Models;

namespace StudyHub.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Course> PopularCourses { get; set; }
            = new List<Course>();

        public async Task OnGetAsync()
        {
            PopularCourses = await _context.Courses
                .Include(c => c.Teacher)
                .Include(c => c.Enrollments)
                .OrderByDescending(c => c.Enrollments.Count)
                .Take(6)
                .ToListAsync();
        }
    }
}