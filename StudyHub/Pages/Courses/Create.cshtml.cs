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
using StudyHub.Models;
namespace StudyHub.Pages_Courses
{
    [Authorize(Roles = "Teacher")]
    public class CreateModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly StudyHub.Data.ApplicationDbContext _context;

        public CreateModel(
    ApplicationDbContext context,
    UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult OnGet()
        {
        ViewData["TeacherId"] = new SelectList(_context.Users, "Id", "Id");
            return Page();
        }

        [BindProperty]
        public Course Course { get; set; } = default!;

        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
            var user = await _userManager.GetUserAsync(User);
            Course.TeacherId = user!.Id;
            _context.Courses.Add(Course);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
