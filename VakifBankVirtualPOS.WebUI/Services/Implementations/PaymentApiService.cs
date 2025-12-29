using VakifBankVirtualPOS.WebUI.Common;
using VakifBankVirtualPOS.WebUI.Models;
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

        public async Task<ApiResponse<EnrollmentResponseDto>> InitiateThreeDSecureAsync(PaymentInitiateViewModel model, CancellationToken cancellationToken = default)
        {
            return await _apiService.PostAsync<EnrollmentResponseDto>($"{BaseEndpoint}/", model, cancellationToken);
        }

        public async Task<ApiResponse<PaymentResultDto>> CompletePaymentAsync(ThreeDCallbackViewModel model, CancellationToken cancellationToken = default)
        {
            return await _apiService.PostAsync<PaymentResultDto>($"{BaseEndpoint}/", model, cancellationToken);
        }
    }
}