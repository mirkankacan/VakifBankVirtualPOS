using VakifBankVirtualPOS.WebUI.Common;
using VakifBankVirtualPOS.WebUI.Models.PaymentViewModels;

namespace VakifBankVirtualPOS.WebUI.Services.Interfaces
{
    public interface IPaymentApiService
    {
        Task<ApiResponse<EnrollmentResponseViewModel>> InitiateThreeDSecureAsync(PaymentInitiateViewModel model, CancellationToken cancellationToken = default);

        Task<ApiResponse<PaymentResultViewModel>> GetPaymentByOrderIdAsync(string orderId, CancellationToken cancellationToken = default);
    }
}