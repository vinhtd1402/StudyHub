using Microsoft.AspNetCore.Identity;

namespace StudyHub.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public decimal WalletBalance { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? TeacherBillingStartsAt { get; set; }
        public DateTime? NextTeacherBillingAt { get; set; }
        public DateTime? LastTeacherBillingNoticeAt { get; set; }
        public bool IsTeacherSuspended { get; set; }
        public DateTime? TeacherSuspendedAt { get; set; }
        public string? TeacherSuspensionReason { get; set; }
    }
}
