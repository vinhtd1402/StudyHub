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
namespace StudyHub.Pages_Lessons
{
    [Authorize(Roles = "Teacher")]
    public class DeleteModel : PageModel
    {
        private readonly StudyHub.Data.ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DeleteModel(
            StudyHub.Data.ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [BindProperty]
        public Lesson Lesson { get; set; } = default!;

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

            var lesson = await _context.Lessons
                .Include(l => l.Course)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (lesson is not null)
            {
                if (lesson.Course?.TeacherId != user.Id)
                {
                    return Forbid();
                }

                Lesson = lesson;

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

            var lesson = await _context.Lessons
                .Include(l => l.Course)
                .FirstOrDefaultAsync(l => l.Id == id);
            if (lesson != null)
            {
                if (lesson.Course?.TeacherId != user.Id)
                {
                    return Forbid();
                }

                Lesson = lesson;
                _context.Lessons.Remove(Lesson);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
