using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StudyHub.Models;
using StudyHub.Services;

namespace StudyHub.Pages.Admin
{
    [Authorize(Policy = "AdminOnly")]
    public class CreditsModel : PageModel
    {
        private readonly WalletService _walletService;

        public CreditsModel(WalletService walletService)
        {
            _walletService = walletService;
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
            Transactions = await _walletService.GetVietQrTransactionsForAdminAsync(
                RoleFilter,
                StatusFilter);
        }

        public async Task<IActionResult> OnPostApproveAsync(int id)
        {
            StatusMessage = await _walletService.ApproveVietQrTransactionAsync(id);
            return RedirectToPage(new { RoleFilter, StatusFilter });
        }

        public async Task<IActionResult> OnPostRejectAsync(int id)
        {
            await _walletService.RejectVietQrTransactionAsync(id);
            StatusMessage = "Transaction rejected.";

            return RedirectToPage(new { RoleFilter, StatusFilter });
        }
    }
}
