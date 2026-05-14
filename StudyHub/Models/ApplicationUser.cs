using Microsoft.AspNetCore.Identity;

namespace StudyHub.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
    }
}