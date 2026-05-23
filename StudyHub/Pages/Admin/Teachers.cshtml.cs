using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StudyHub.Models;

namespace StudyHub.Pages.Admin
{
    [Authorize(Policy = "AdminOnly")]
    public class TeachersModel : PageModel
    {
        private const string TeacherRole = "Teacher";

        private readonly UserManager<ApplicationUser> _userManager;

        public TeachersModel(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public IList<ApplicationUser> Teachers { get; set; } = new List<ApplicationUser>();

        [TempData]
        public string? StatusMessage { get; set; }

        public async Task OnGetAsync()
        {
            Teachers = await _userManager.GetUsersInRoleAsync(TeacherRole);
        }
    }
}
