using VakifBankVirtualPOS.WebUI.Common;
using VakifBankVirtualPOS.WebUI.Models.PaymentViewModels;
using VakifBankVirtualPOS.WebUI.Services.Interfaces;

namespace VakifBankVirtualPOS.WebUI.Services.Implementations
{
    public class PaymentApiService : IPaymentApiService
    {
        private readonly IApiService _apiService;
        private readonly ILogger<PaymentApiService> _logger;

        public PaymentApiService(
            IApiService apiService,
            ILogger<PaymentApiService> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        private const string BaseEndpoint = "api/payments";

        public async Task<ApiResponse<EnrollmentResponseViewModel>> InitiateThreeDSecureAsync(PaymentInitiateViewModel model, CancellationToken cancellationToken = default)
        {
            return await _apiService.PostAsync<EnrollmentResponseViewModel>($"{BaseEndpoint}/initiate", model, cancellationToken);
        }

        public async Task<ApiResponse<PaymentResultViewModel>> GetPaymentByOrderIdAsync(string orderId, CancellationToken cancellationToken = default)
        {
            return await _apiService.GetAsync<PaymentResultViewModel>($"{BaseEndpoint}/{orderId}", cancellationToken);
        }
    }
}