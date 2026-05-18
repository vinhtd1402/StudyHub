using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using StudyHub.Data;
using StudyHub.Models;

namespace StudyHub.Pages.Courses
{
    [Authorize(Roles = "Student")]
    public class MyCoursesModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public MyCoursesModel(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public List<Course> Courses { get; set; } = new();

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null) return;

            Courses = await _context.Enrollments
                .Where(e => e.StudentId == user.Id)
                .Include(e => e.Course)
                .Select(e => e.Course)
                .ToListAsync();
        }
    }
}