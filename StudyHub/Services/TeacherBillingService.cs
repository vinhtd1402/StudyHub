using Microsoft.AspNetCore.Identity;
using StudyHub.Data;
using StudyHub.Models;

namespace StudyHub.Services
{
    public class TeacherBillingService
    {
        public const decimal MonthlyFee = 100000m;

        private const string TeacherRole = "Teacher";
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly StudyHubEmailSender _emailSender;
        private readonly ILogger<TeacherBillingService> _logger;

        public TeacherBillingService(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            StudyHubEmailSender emailSender,
            ILogger<TeacherBillingService> logger)
        {
            _context = context;
            _userManager = userManager;
            _emailSender = emailSender;
            _logger = logger;
        }

        public async Task EnsureTeacherBillingProfileAsync(
            ApplicationUser teacher,
            CancellationToken cancellationToken = default)
        {
            var changed = false;

            if (teacher.TeacherBillingStartsAt == null)
            {
                teacher.TeacherBillingStartsAt = teacher.CreatedAt.AddDays(30);
                changed = true;
            }

            if (teacher.NextTeacherBillingAt == null)
            {
                teacher.NextTeacherBillingAt = teacher.TeacherBillingStartsAt;
                changed = true;
            }

            if (changed)
            {
                await _userManager.UpdateAsync(teacher);
            }
        }

        public async Task ProcessDueTeachersAsync(CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            var teachers = await _userManager.GetUsersInRoleAsync(TeacherRole);

            foreach (var teacher in teachers)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await EnsureTeacherBillingProfileAsync(teacher, cancellationToken);

                if (teacher.NextTeacherBillingAt == null || teacher.IsTeacherSuspended)
                {
                    continue;
                }

                await SendBillingNoticeIfNeededAsync(teacher, now, cancellationToken);

                if (teacher.NextTeacherBillingAt <= now)
                {
                    await ChargeTeacherAsync(teacher, now, cancellationToken);
                }
            }
        }

        private async Task SendBillingNoticeIfNeededAsync(
            ApplicationUser teacher,
            DateTime now,
            CancellationToken cancellationToken)
        {
            if (teacher.NextTeacherBillingAt == null)
            {
                return;
            }

            var noticeDate = teacher.NextTeacherBillingAt.Value.AddDays(-1);

            if (now < noticeDate ||
                teacher.LastTeacherBillingNoticeAt?.Date == now.Date ||
                string.IsNullOrWhiteSpace(teacher.Email))
            {
                return;
            }

            await _emailSender.SendAsync(
                teacher.Email,
                "StudyHub teacher monthly fee reminder",
                $"StudyHub will charge {MonthlyFee:N0} VND from your wallet on {teacher.NextTeacherBillingAt.Value:dd/MM/yyyy}. Please keep enough balance to keep your teacher account active.",
                cancellationToken);

            teacher.LastTeacherBillingNoticeAt = now;
            await _userManager.UpdateAsync(teacher);
        }

        private async Task ChargeTeacherAsync(
            ApplicationUser teacher,
            DateTime now,
            CancellationToken cancellationToken)
        {
            if (teacher.WalletBalance >= MonthlyFee)
            {
                teacher.WalletBalance -= MonthlyFee;
                teacher.NextTeacherBillingAt = teacher.NextTeacherBillingAt!.Value.AddMonths(1);
                teacher.LastTeacherBillingNoticeAt = null;

                _context.CreditTransactions.Add(new CreditTransaction
                {
                    UserId = teacher.Id,
                    Amount = -MonthlyFee,
                    OrderId = $"FEE{Guid.NewGuid():N}",
                    RequestId = $"FEE-{Guid.NewGuid():N}",
                    Provider = "StudyHub",
                    Status = CreditTransactionStatus.Paid,
                    Message = "Teacher monthly maintenance fee",
                    PaidAt = now
                });

                await _context.SaveChangesAsync(cancellationToken);
                await _userManager.UpdateAsync(teacher);
                return;
            }

            teacher.IsTeacherSuspended = true;
            teacher.TeacherSuspendedAt = now;
            teacher.TeacherSuspensionReason = "Insufficient wallet balance for the teacher monthly maintenance fee.";
            teacher.LockoutEnabled = true;
            teacher.LockoutEnd = DateTimeOffset.UtcNow.AddYears(100);

            await _userManager.UpdateAsync(teacher);

            if (!string.IsNullOrWhiteSpace(teacher.Email))
            {
                await _emailSender.SendAsync(
                    teacher.Email,
                    "StudyHub teacher account temporarily locked",
                    "Your StudyHub teacher account has been temporarily locked because your wallet does not have enough balance for the monthly maintenance fee. Please contact an admin to reopen the account.",
                    cancellationToken);
            }

            _logger.LogInformation("Teacher {TeacherId} was suspended for insufficient wallet balance.", teacher.Id);
        }
    }
}
