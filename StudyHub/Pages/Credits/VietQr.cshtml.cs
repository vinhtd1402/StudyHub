using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StudyHub.Models;
using StudyHub.Services;

namespace StudyHub.Pages.Credits
{
    [Authorize(Roles = "Student,Teacher")]
    public class VietQrModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly VietQrPaymentService _vietQrPaymentService;
        private readonly WalletService _walletService;

        public VietQrModel(
            UserManager<ApplicationUser> userManager,
            VietQrPaymentService vietQrPaymentService,
            WalletService walletService)
        {
            _userManager = userManager;
            _vietQrPaymentService = vietQrPaymentService;
            _walletService = walletService;
        }

        public CreditTransaction Transaction { get; set; } = new();
        public string BankId => _vietQrPaymentService.BankId;
        public string AccountNo => _vietQrPaymentService.AccountNo;

        public async Task<IActionResult> OnGetAsync(string orderId)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Challenge();
            }

            var transaction = await _walletService.GetUserVietQrTransactionAsync(user.Id, orderId);

            if (transaction == null)
            {
                return NotFound();
            }

            Transaction = transaction;
            return Page();
        }
    }
}
