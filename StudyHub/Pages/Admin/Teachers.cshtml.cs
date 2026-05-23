using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StudyHub.Models;
using StudyHub.Services;

namespace StudyHub.Pages.Admin
{
    [Authorize(Policy = "AdminOnly")]
    public class TeachersModel : PageModel
    {
        private const string TeacherRole = "Teacher";

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly TeacherBillingService _teacherBillingService;

        public TeachersModel(
            UserManager<ApplicationUser> userManager,
            TeacherBillingService teacherBillingService)
        {
            _userManager = userManager;
            _teacherBillingService = teacherBillingService;
        }

        public IList<ApplicationUser> Teachers { get; set; } = new List<ApplicationUser>();

        [BindProperty(SupportsGet = true)]
        public string? StatusFilter { get; set; }

        [TempData]
        public string? StatusMessage { get; set; }

        public async Task OnGetAsync()
        {
            var teachers = await _userManager.GetUsersInRoleAsync(TeacherRole);

            foreach (var teacher in teachers)
            {
                await _teacherBillingService.EnsureTeacherBillingProfileAsync(teacher);
            }

            Teachers = StatusFilter switch
            {
                "Suspended" => teachers.Where(t => t.IsTeacherSuspended).ToList(),
                "Active" => teachers.Where(t => !t.IsTeacherSuspended).ToList(),
                _ => teachers
            };
        }

        public async Task<IActionResult> OnPostUnlockAsync(string id)
        {
            var teacher = await _userManager.FindByIdAsync(id);

            if (teacher == null || !await _userManager.IsInRoleAsync(teacher, TeacherRole))
            {
                return NotFound();
            }

            teacher.IsTeacherSuspended = false;
            teacher.TeacherSuspendedAt = null;
            teacher.TeacherSuspensionReason = null;
            teacher.LockoutEnd = null;
            teacher.LockoutEnabled = true;

            await _teacherBillingService.EnsureTeacherBillingProfileAsync(teacher);
            await _userManager.UpdateAsync(teacher);

            StatusMessage = $"Teacher account '{teacher.Email}' was reopened.";
            return RedirectToPage(new { StatusFilter });
        }
    }
}
