using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using StudyHub.Data;
using StudyHub.Models;

namespace StudyHub.Pages.Admin
{
    [Authorize(Policy = "AdminOnly")]
    public class CreditsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CreditsModel(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IList<CreditTransaction> Transactions { get; set; } = new List<CreditTransaction>();

        [BindProperty(SupportsGet = true)]
        public string? RoleFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? StatusFilter { get; set; } = CreditTransactionStatus.Pending;

        [TempData]
        public string? StatusMessage { get; set; }

        public async Task OnGetAsync()
        {
            var query = _context.CreditTransactions
                .Include(t => t.User)
                .Where(t => t.Provider == "VietQR")
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(StatusFilter))
            {
                query = query.Where(t => t.Status == StatusFilter);
            }

            var transactions = await query
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            if (!string.IsNullOrWhiteSpace(RoleFilter))
            {
                var filtered = new List<CreditTransaction>();

                foreach (var transaction in transactions)
                {
                    if (transaction.User != null &&
                        await _userManager.IsInRoleAsync(transaction.User, RoleFilter))
                    {
                        filtered.Add(transaction);
                    }
                }

                transactions = filtered;
            }

            Transactions = transactions;
        }

        public async Task<IActionResult> OnPostApproveAsync(int id)
        {
            var transaction = await _context.CreditTransactions
                .Include(t => t.User)
                .FirstOrDefaultAsync(t =>
                    t.Id == id &&
                    t.Provider == "VietQR");

            if (transaction == null || transaction.User == null)
            {
                return NotFound();
            }

            if (transaction.Status == CreditTransactionStatus.Paid)
            {
                StatusMessage = "This transaction was already approved.";
                return RedirectToPage(new { RoleFilter, StatusFilter });
            }

            transaction.Status = CreditTransactionStatus.Paid;
            transaction.PaidAt = DateTime.UtcNow;
            transaction.Message = "Approved by admin.";
            transaction.User.WalletBalance += transaction.Amount;

            await _context.SaveChangesAsync();

            StatusMessage = $"Approved {transaction.Amount:N0} VND for {transaction.User.Email}.";
            return RedirectToPage(new { RoleFilter, StatusFilter });
        }

        public async Task<IActionResult> OnPostRejectAsync(int id)
        {
            var transaction = await _context.CreditTransactions
                .FirstOrDefaultAsync(t =>
                    t.Id == id &&
                    t.Provider == "VietQR");

            if (transaction == null)
            {
                return NotFound();
            }

            if (transaction.Status == CreditTransactionStatus.Pending)
            {
                transaction.Status = CreditTransactionStatus.Failed;
                transaction.Message = "Rejected by admin.";
                await _context.SaveChangesAsync();
                StatusMessage = "Transaction rejected.";
            }

            return RedirectToPage(new { RoleFilter, StatusFilter });
        }
    }
}
