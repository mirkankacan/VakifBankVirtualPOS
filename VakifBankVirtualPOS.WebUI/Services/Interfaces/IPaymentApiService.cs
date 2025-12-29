using VakifBankVirtualPOS.WebUI.Common;
using VakifBankVirtualPOS.WebUI.Models;

namespace VakifBankVirtualPOS.WebUI.Services.Interfaces
{
    public interface IPaymentApiService
    {
        Task<ApiResponse<EnrollmentResponseDto>> InitiateThreeDSecureAsync(PaymentInitiateViewModel model, CancellationToken cancellationToken = default);
        Task<ApiResponse<PaymentResultDto>> CompletePaymentAsync(ThreeDCallbackViewModel model, CancellationToken cancellationToken = default);
    }

    public class EnrollmentResponseDto
    {
        public string OrderId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string MessageErrorCode { get; set; } = string.Empty;
        public string ACSUrl { get; set; } = string.Empty;
        public string PAReq { get; set; } = string.Empty;
        public string TermUrl { get; set; } = string.Empty;
        public string MD { get; set; } = string.Empty;
    }

    public class PaymentResultDto
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public string TransactionId { get; set; } = string.Empty;
        public string AuthCode { get; set; } = string.Empty;
        public string OrderId { get; set; } = string.Empty;
        public string ErrorCode { get; set; } = string.Empty;
    }
}