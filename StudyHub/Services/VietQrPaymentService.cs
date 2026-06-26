namespace StudyHub.Services
{
    public class VietQrPaymentService
    {
        private readonly VietQrOptions _options;

        public VietQrPaymentService(Microsoft.Extensions.Options.IOptions<VietQrOptions> options)
        {
            _options = options.Value;
        }

        public bool IsConfigured =>
            !string.IsNullOrWhiteSpace(_options.BankId) &&
            !string.IsNullOrWhiteSpace(_options.AccountNo) &&
            !string.IsNullOrWhiteSpace(_options.Template);

        public string BankId => _options.BankId;
        public string AccountNo => _options.AccountNo;

        public string CreateQrImageUrl(decimal amount, string addInfo)
        {
            if (!IsConfigured)
            {
                throw new InvalidOperationException("VietQR payment is not configured.");
            }

            var amountValue = decimal.ToInt64(decimal.Truncate(amount));
            var encodedInfo = Uri.EscapeDataString(addInfo);

            return $"https://img.vietqr.io/image/{_options.BankId}-{_options.AccountNo}-{_options.Template}.png?amount={amountValue}&addInfo={encodedInfo}";
        }
    }
}
