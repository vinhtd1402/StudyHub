using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StudyHub.Models;

namespace StudyHub.Pages.Admin
{
    [Authorize(Policy = "AdminOnly")]
    public class CreateTeacherModel : PageModel
    {
        private const string TeacherRole = "Teacher";

        private readonly UserManager<ApplicationUser> _userManager;

        public CreateTeacherModel(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        [TempData]
        public string? StatusMessage { get; set; }

        public class InputModel
        {
            [Required]
            [Display(Name = "Full Name")]
            public string FullName { get; set; } = string.Empty;

            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; } = string.Empty;

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; } = string.Empty;

            [DataType(DataType.Password)]
            [Display(Name = "Confirm Password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; } = string.Empty;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var existingUser = await _userManager.FindByEmailAsync(Input.Email);

            if (existingUser != null)
            {
                ModelState.AddModelError(string.Empty, "A user with this email already exists.");
                return Page();
            }

            var teacher = new ApplicationUser
            {
                UserName = Input.Email,
                Email = Input.Email,
                FullName = Input.FullName,
                EmailConfirmed = true
            };

            var createResult = await _userManager.CreateAsync(teacher, Input.Password);

            if (!createResult.Succeeded)
            {
                AddIdentityErrors(createResult);
                return Page();
            }

            var roleResult = await _userManager.AddToRoleAsync(teacher, TeacherRole);

            if (!roleResult.Succeeded)
            {
                AddIdentityErrors(roleResult);
                return Page();
            }

            TempData["StatusMessage"] = $"Teacher account '{Input.Email}' was created.";
            return RedirectToPage("/Admin/Teachers");
        }

        private void AddIdentityErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }
    }
}
