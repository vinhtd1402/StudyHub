using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using StudyHub.Data;
using StudyHub.Models;
using Microsoft.AspNetCore.Identity;
using StudyHub.Models;
namespace StudyHub.Pages_Courses
{
    public class DetailsModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly StudyHub.Data.ApplicationDbContext _context;

        public DetailsModel(
    ApplicationDbContext context,
    UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public Course Course { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Courses
    .Include(c => c.Lessons)
    .FirstOrDefaultAsync(m => m.Id == id);

            if (course is not null)
            {
                Course = course;

                return Page();
            }

            return NotFound();
        }
        
        public async Task<IActionResult> OnPostEnrollAsync(int? id)
        {
            if (id == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);

            if (user == null) return Challenge();

            var exists = await _context.Enrollments
                .AnyAsync(e => e.CourseId == id && e.StudentId == user.Id);

            if (!exists)
            {
                var enrollment = new Enrollment
                {
                    CourseId = id.Value,
                    StudentId = user.Id,
                    EnrolledAt = DateTime.UtcNow
                };

                _context.Enrollments.Add(enrollment);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage(new { id });
        }
    }
}
