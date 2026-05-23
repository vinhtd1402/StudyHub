using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using StudyHub.Data;
using StudyHub.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using StudyHub.Services;
namespace StudyHub.Pages_Courses
{
    [Authorize(Roles = "Teacher")]
    public class DeleteModel : PageModel
    {
        private readonly StudyHub.Data.ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AccessControlService _accessControlService;

        public DeleteModel(
            StudyHub.Data.ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            AccessControlService accessControlService)
        {
            _context = context;
            _userManager = userManager;
            _accessControlService = accessControlService;
        }

        [BindProperty]
        public Course Course { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Challenge();
            }

            var course = await _context.Courses.FirstOrDefaultAsync(m => m.Id == id);

            if (course is not null)
            {
                if (!await _accessControlService.TeacherOwnsCourseAsync(user.Id, course.Id))
                {
                    return Forbid();
                }

                Course = course;

                return Page();
            }

            return NotFound();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Challenge();
            }

            var course = await _context.Courses.FindAsync(id);
            if (course != null)
            {
                if (!await _accessControlService.TeacherOwnsCourseAsync(user.Id, course.Id))
                {
                    return Forbid();
                }

                Course = course;
                _context.Courses.Remove(Course);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
