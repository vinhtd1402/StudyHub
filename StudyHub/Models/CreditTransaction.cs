using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudyHub.Models
{
    public class CreditTransaction
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        public ApplicationUser? User { get; set; }

        [Column(TypeName = "decimal(18,0)")]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(200)]
        public string OrderId { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string RequestId { get; set; } = string.Empty;

        [StringLength(50)]
        public string Provider { get; set; } = "MoMo";

        [StringLength(30)]
        public string Status { get; set; } = CreditTransactionStatus.Pending;

        public long? MomoTransactionId { get; set; }

        public int? ResultCode { get; set; }

        [StringLength(500)]
        public string? Message { get; set; }

        [StringLength(1000)]
        public string? PayUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? PaidAt { get; set; }
    }
}
