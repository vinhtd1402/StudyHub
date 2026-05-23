using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StudyHub.Models;
using StudyHub.Services;

namespace StudyHub.Pages.Credits
{
    [Authorize(Roles = "Student,Teacher")]
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly VietQrPaymentService _vietQrPaymentService;
        private readonly WalletService _walletService;

        public IndexModel(
            UserManager<ApplicationUser> userManager,
            VietQrPaymentService vietQrPaymentService,
            WalletService walletService)
        {
            _userManager = userManager;
            _vietQrPaymentService = vietQrPaymentService;
            _walletService = walletService;
        }

        public decimal WalletBalance { get; set; }
        public IList<CreditTransaction> Transactions { get; set; } = new List<CreditTransaction>();
        public bool IsVietQrConfigured => _vietQrPaymentService.IsConfigured;
        public string VietQrBankId => _vietQrPaymentService.BankId;
        public string VietQrAccountNo => _vietQrPaymentService.AccountNo;

        [BindProperty]
        public InputModel Input { get; set; } = new();

        [TempData]
        public string? StatusMessage { get; set; }

        public class InputModel
        {
            [Required]
            [Range(1000, 50000000, ErrorMessage = "Amount must be from 1,000 to 50,000,000 VND.")]
            [Display(Name = "Amount")]
            public long Amount { get; set; } = 10000;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Challenge();
            }

            await LoadWalletAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostVietQrTopUpAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Challenge();
            }

            if (!ModelState.IsValid)
            {
                await LoadWalletAsync(user);
                return Page();
            }

            if (!_vietQrPaymentService.IsConfigured)
            {
                ModelState.AddModelError(string.Empty, "VietQR is not configured. Please add BankId, AccountNo, and Template in appsettings.json or user secrets.");
                await LoadWalletAsync(user);
                return Page();
            }

            var transaction = await _walletService.CreateVietQrTopUpAsync(user, Input.Amount);

            return RedirectToPage("/Credits/VietQr", new { transaction.OrderId });
        }

        private async Task LoadWalletAsync(ApplicationUser user)
        {
            WalletBalance = user.WalletBalance;

            Transactions = await _walletService.GetRecentTransactionsAsync(user.Id);
        }
    }
}
