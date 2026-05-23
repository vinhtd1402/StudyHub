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
    [AllowAnonymous]
    [IgnoreAntiforgeryToken]
    public class MomoReturnModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly MomoPaymentService _momoPaymentService;

        public MomoReturnModel(
            ApplicationDbContext context,
            MomoPaymentService momoPaymentService)
        {
            _context = context;
            _momoPaymentService = momoPaymentService;
        }

        public string StatusTitle { get; set; } = "Payment result";
        public string StatusMessage { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync()
        {
            var result = ReadResultFromRequest();
            await ProcessPaymentResultAsync(result);

            return Page();
        }

        public async Task<IActionResult> OnPostIpnAsync()
        {
            var result = ReadResultFromRequest();
            await ProcessPaymentResultAsync(result);

            return new JsonResult(new
            {
                resultCode = 0,
                message = "Received"
            });
        }

        private MomoPaymentResult ReadResultFromRequest()
        {
            return new MomoPaymentResult
            {
                PartnerCode = GetRequestValue("partnerCode"),
                OrderId = GetRequestValue("orderId"),
                RequestId = GetRequestValue("requestId"),
                Amount = ReadLong(GetRequestValue("amount")),
                OrderInfo = GetRequestValue("orderInfo"),
                OrderType = GetRequestValue("orderType"),
                TransId = ReadNullableLong(GetRequestValue("transId")),
                ResultCode = ReadInt(GetRequestValue("resultCode")),
                Message = GetRequestValue("message"),
                PayType = GetRequestValue("payType"),
                ResponseTime = ReadLong(GetRequestValue("responseTime")),
                ExtraData = GetRequestValue("extraData"),
                Signature = GetRequestValue("signature")
            };
        }

        private string GetRequestValue(string key)
        {
            if (Request.HasFormContentType && Request.Form.TryGetValue(key, out var formValue))
            {
                return formValue.ToString();
            }

            return Request.Query.TryGetValue(key, out var queryValue)
                ? queryValue.ToString()
                : string.Empty;
        }

        private async Task ProcessPaymentResultAsync(MomoPaymentResult result)
        {
            if (string.IsNullOrWhiteSpace(result.OrderId))
            {
                StatusTitle = "Missing payment information";
                StatusMessage = "MoMo did not return an order id.";
                return;
            }

            var transaction = await _context.CreditTransactions
                .Include(t => t.User)
                .FirstOrDefaultAsync(t =>
                    t.OrderId == result.OrderId &&
                    t.RequestId == result.RequestId);

            if (transaction == null || transaction.User == null)
            {
                StatusTitle = "Payment not found";
                StatusMessage = "StudyHub could not find this MoMo transaction.";
                return;
            }

            if (!_momoPaymentService.VerifyPaymentResult(result))
            {
                transaction.Status = CreditTransactionStatus.Failed;
                transaction.ResultCode = result.ResultCode;
                transaction.Message = "Invalid MoMo signature.";
                await _context.SaveChangesAsync();

                StatusTitle = "Payment verification failed";
                StatusMessage = "MoMo signature is invalid.";
                return;
            }

            transaction.ResultCode = result.ResultCode;
            transaction.Message = result.Message;
            transaction.MomoTransactionId = result.TransId;

            if (result.IsSuccess && transaction.Status != CreditTransactionStatus.Paid)
            {
                transaction.Status = CreditTransactionStatus.Paid;
                transaction.PaidAt = DateTime.UtcNow;
                transaction.User.WalletBalance += transaction.Amount;

                StatusTitle = "Top-up successful";
                StatusMessage = $"{transaction.Amount:N0} VND has been added to your StudyHub wallet.";
            }
            else if (!result.IsSuccess)
            {
                transaction.Status = CreditTransactionStatus.Failed;

                StatusTitle = "Top-up failed";
                StatusMessage = result.Message;
            }
            else
            {
                StatusTitle = "Top-up already processed";
                StatusMessage = "This transaction was already added to your wallet.";
            }

            await _context.SaveChangesAsync();
        }

        private static int ReadInt(string value)
        {
            return int.TryParse(value, out var result) ? result : -1;
        }

        private static long ReadLong(string value)
        {
            return long.TryParse(value, out var result) ? result : 0;
        }

        private static long? ReadNullableLong(string value)
        {
            return long.TryParse(value, out var result) ? result : null;
        }
    }
}
