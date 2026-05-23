using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using StudyHub.Data;
using StudyHub.Models;
using StudyHub.Services;

namespace StudyHub.Pages.Credits
{
    [Authorize(Roles = "Student,Teacher")]
    public class VietQrModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly VietQrPaymentService _vietQrPaymentService;

        public VietQrModel(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            VietQrPaymentService vietQrPaymentService)
        {
            _context = context;
            _userManager = userManager;
            _vietQrPaymentService = vietQrPaymentService;
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

            var transaction = await _context.CreditTransactions
                .FirstOrDefaultAsync(t =>
                    t.UserId == user.Id &&
                    t.OrderId == orderId &&
                    t.Provider == "VietQR");

            if (transaction == null)
            {
                return NotFound();
            }

            Transaction = transaction;
            return Page();
        }
    }
}
