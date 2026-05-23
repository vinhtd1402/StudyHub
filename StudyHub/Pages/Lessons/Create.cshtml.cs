using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using StudyHub.Data;
using StudyHub.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace StudyHub.Pages_Lessons
{
    [Authorize(Roles = "Teacher")]
    public class CreateModel : PageModel
    {
        private readonly StudyHub.Data.ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CreateModel(
            StudyHub.Data.ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Challenge();
            }

            if (user.IsTeacherSuspended)
            {
                return Forbid();
            }

            await LoadCourseOptionsAsync(user.Id);
            return Page();
        }

        [BindProperty]
        public Lesson Lesson { get; set; } = default!;

        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser != null)
                {
                    await LoadCourseOptionsAsync(currentUser.Id);
                }

                return Page();
            }

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Challenge();
            }

            if (user.IsTeacherSuspended)
            {
                return Forbid();
            }

            var ownsCourse = await _context.Courses
                .AnyAsync(c => c.Id == Lesson.CourseId && c.TeacherId == user.Id);

            if (!ownsCourse)
            {
                return Forbid();
            }

            _context.Lessons.Add(Lesson);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }

        private async Task LoadCourseOptionsAsync(string teacherId)
        {
            var courses = await _context.Courses
                .Where(c => c.TeacherId == teacherId)
                .OrderBy(c => c.Title)
                .ToListAsync();

            ViewData["CourseId"] = new SelectList(courses, "Id", "Title");
        }
    }
}
