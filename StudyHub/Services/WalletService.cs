using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StudyHub.Data.Repositories;
using StudyHub.Models;

namespace StudyHub.Services
{
    public class WalletService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly VietQrPaymentService _vietQrPaymentService;

        public WalletService(
            IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager,
            VietQrPaymentService vietQrPaymentService)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _vietQrPaymentService = vietQrPaymentService;
        }

        public async Task<IList<CreditTransaction>> GetRecentTransactionsAsync(string userId, int count = 10)
        {
            return await _unitOfWork.CreditTransactions.Query()
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<CreditTransaction> CreateVietQrTopUpAsync(ApplicationUser user, long amount)
        {
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var orderId = $"VQR{timestamp}";
            var requestId = $"VQR-{timestamp}-{Guid.NewGuid():N}";
            var qrImageUrl = _vietQrPaymentService.CreateQrImageUrl(amount, orderId);

            var transaction = new CreditTransaction
            {
                UserId = user.Id,
                Amount = amount,
                OrderId = orderId,
                RequestId = requestId,
                Provider = "VietQR",
                PayUrl = qrImageUrl,
                Message = $"Transfer content: {orderId}"
            };

            await _unitOfWork.CreditTransactions.AddAsync(transaction);
            await _unitOfWork.SaveChangesAsync();

            return transaction;
        }

        public async Task<CreditTransaction?> GetUserVietQrTransactionAsync(string userId, string orderId)
        {
            return await _unitOfWork.CreditTransactions.Query()
                .FirstOrDefaultAsync(t =>
                    t.UserId == userId &&
                    t.OrderId == orderId &&
                    t.Provider == "VietQR");
        }

        public async Task<IList<CreditTransaction>> GetVietQrTransactionsForAdminAsync(
            string? roleFilter,
            string? statusFilter)
        {
            var query = _unitOfWork.CreditTransactions.Query()
                .Include(t => t.User)
                .Where(t => t.Provider == "VietQR");

            if (!string.IsNullOrWhiteSpace(statusFilter))
            {
                query = query.Where(t => t.Status == statusFilter);
            }

            var transactions = await query
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            if (string.IsNullOrWhiteSpace(roleFilter))
            {
                return transactions;
            }

            var filtered = new List<CreditTransaction>();

            foreach (var transaction in transactions)
            {
                if (transaction.User != null &&
                    await _userManager.IsInRoleAsync(transaction.User, roleFilter))
                {
                    filtered.Add(transaction);
                }
            }

            return filtered;
        }

        public async Task<string> ApproveVietQrTransactionAsync(int id)
        {
            var transaction = await _unitOfWork.CreditTransactions.Query()
                .Include(t => t.User)
                .FirstOrDefaultAsync(t =>
                    t.Id == id &&
                    t.Provider == "VietQR");

            if (transaction == null || transaction.User == null)
            {
                throw new InvalidOperationException("Transaction not found.");
            }

            if (transaction.Status == CreditTransactionStatus.Paid)
            {
                return "This transaction was already approved.";
            }

            transaction.Status = CreditTransactionStatus.Paid;
            transaction.PaidAt = DateTime.UtcNow;
            transaction.Message = "Approved by admin.";
            transaction.User.WalletBalance += transaction.Amount;

            await _unitOfWork.SaveChangesAsync();

            return $"Approved {transaction.Amount:N0} VND for {transaction.User.Email}.";
        }

        public async Task RejectVietQrTransactionAsync(int id)
        {
            var transaction = await _unitOfWork.CreditTransactions.Query()
                .FirstOrDefaultAsync(t =>
                    t.Id == id &&
                    t.Provider == "VietQR");

            if (transaction == null)
            {
                throw new InvalidOperationException("Transaction not found.");
            }

            if (transaction.Status != CreditTransactionStatus.Pending)
            {
                return;
            }

            transaction.Status = CreditTransactionStatus.Failed;
            transaction.Message = "Rejected by admin.";

            await _unitOfWork.SaveChangesAsync();
        }
    }
}
