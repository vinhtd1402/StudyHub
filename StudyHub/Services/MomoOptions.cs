namespace StudyHub.Services
{
    public class MomoOptions
    {
        public string Endpoint { get; set; } = "https://test-payment.momo.vn/v2/gateway/api/create";
        public string PartnerCode { get; set; } = string.Empty;
        public string AccessKey { get; set; } = string.Empty;
        public string SecretKey { get; set; } = string.Empty;
        public string Lang { get; set; } = "vi";
    }
}
