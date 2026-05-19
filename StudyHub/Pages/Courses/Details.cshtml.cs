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
using System.Security.Claims;
namespace StudyHub.Pages_Courses
{
    public class DetailsModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly StudyHub.Data.ApplicationDbContext _context;
        public IList<ApplicationUser> EnrolledStudents { get; set; } = new List<ApplicationUser>();
        public bool IsEnrolled { get; set; }
        public Course Course { get; set; }

        public DetailsModel(
    ApplicationDbContext context,
    UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        public async Task<IActionResult> OnGetAsync(int id)
        {
            Course = await _context.Courses
                .Include(c => c.Teacher) 
                .Include(c => c.Lessons)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (Course == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            IsEnrolled = await _context.Enrollments
                .AnyAsync(e => e.CourseId == id && e.StudentId == userId);

            EnrolledStudents = await _context.Enrollments
    .Where(e => e.CourseId == id && e.Student != null)
    .Include(e => e.Student)
    .Select(e => e.Student!)
    .ToListAsync();

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
        public async Task<IActionResult> OnPostUnenrollAsync(int id)
        {
            if (!User.IsInRole("Student")) // 🔥 THÊM
            {
                return Forbid();
            }

            var user = await _userManager.GetUserAsync(User);

            if (user == null) return Challenge();

            var enrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.CourseId == id && e.StudentId == user.Id);

            if (enrollment != null)
            {
                _context.Enrollments.Remove(enrollment);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage(new { id });
        }
    }
}
