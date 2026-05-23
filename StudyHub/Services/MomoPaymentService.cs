using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace StudyHub.Services
{
    public class MomoPaymentService
    {
        private const string RequestType = "captureWallet";

        private readonly HttpClient _httpClient;
        private readonly MomoOptions _options;

        public MomoPaymentService(HttpClient httpClient, IOptions<MomoOptions> options)
        {
            _httpClient = httpClient;
            _options = options.Value;
        }

        public bool IsConfigured =>
            !string.IsNullOrWhiteSpace(_options.PartnerCode) &&
            !string.IsNullOrWhiteSpace(_options.AccessKey) &&
            !string.IsNullOrWhiteSpace(_options.SecretKey);

        public async Task<MomoCreatePaymentResponse> CreatePaymentAsync(
            MomoCreatePaymentRequest request,
            CancellationToken cancellationToken = default)
        {
            if (!IsConfigured)
            {
                throw new InvalidOperationException("MoMo payment is not configured.");
            }

            var signature = CreateRequestSignature(request);

            var payload = new
            {
                partnerCode = _options.PartnerCode,
                requestType = RequestType,
                ipnUrl = request.IpnUrl,
                redirectUrl = request.RedirectUrl,
                orderId = request.OrderId,
                amount = request.Amount,
                orderInfo = request.OrderInfo,
                requestId = request.RequestId,
                extraData = request.ExtraData,
                signature,
                lang = _options.Lang
            };

            using var response = await _httpClient.PostAsJsonAsync(
                _options.Endpoint,
                payload,
                cancellationToken);

            var content = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"MoMo create payment failed: {content}");
            }

            return JsonSerializer.Deserialize<MomoCreatePaymentResponse>(
                content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? throw new InvalidOperationException("MoMo create payment returned an empty response.");
        }

        public bool VerifyPaymentResult(MomoPaymentResult result)
        {
            if (!IsConfigured)
            {
                return false;
            }

            var rawSignature =
                $"accessKey={_options.AccessKey}" +
                $"&amount={result.Amount}" +
                $"&extraData={result.ExtraData}" +
                $"&message={result.Message}" +
                $"&orderId={result.OrderId}" +
                $"&orderInfo={result.OrderInfo}" +
                $"&orderType={result.OrderType}" +
                $"&partnerCode={result.PartnerCode}" +
                $"&payType={result.PayType}" +
                $"&requestId={result.RequestId}" +
                $"&responseTime={result.ResponseTime}" +
                $"&resultCode={result.ResultCode}" +
                $"&transId={result.TransId}";

            return string.Equals(
                CreateSignature(rawSignature),
                result.Signature,
                StringComparison.OrdinalIgnoreCase);
        }

        private string CreateRequestSignature(MomoCreatePaymentRequest request)
        {
            var rawSignature =
                $"accessKey={_options.AccessKey}" +
                $"&amount={request.Amount}" +
                $"&extraData={request.ExtraData}" +
                $"&ipnUrl={request.IpnUrl}" +
                $"&orderId={request.OrderId}" +
                $"&orderInfo={request.OrderInfo}" +
                $"&partnerCode={_options.PartnerCode}" +
                $"&redirectUrl={request.RedirectUrl}" +
                $"&requestId={request.RequestId}" +
                $"&requestType={RequestType}";

            return CreateSignature(rawSignature);
        }

        private string CreateSignature(string rawSignature)
        {
            var keyBytes = Encoding.UTF8.GetBytes(_options.SecretKey);
            var dataBytes = Encoding.UTF8.GetBytes(rawSignature);

            using var hmac = new HMACSHA256(keyBytes);
            var hash = hmac.ComputeHash(dataBytes);

            return Convert.ToHexString(hash).ToLowerInvariant();
        }
    }
}
