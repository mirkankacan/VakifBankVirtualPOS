using VakifBankVirtualPOS.WebUI.Common;
using VakifBankVirtualPOS.WebUI.Models.ClientViewModels;
using VakifBankVirtualPOS.WebUI.Services.Interfaces;

namespace VakifBankVirtualPOS.WebUI.Services.Implementations
{
    public class ClientApiService : IClientApiService
    {
        private readonly IApiService _apiService;
        private readonly ILogger<ClientApiService> _logger;

        public ClientApiService(
            IApiService apiService,
            ILogger<ClientApiService> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        private const string BaseEndpoint = "api/clients";

        public async Task<ApiResponse<ClientViewModel>> CheckByNoAsync(string no, CancellationToken cancellationToken = default)
        {
            return await _apiService.GetAsync<ClientViewModel>($"{BaseEndpoint}/check/{no}", cancellationToken);
        }

        public async Task<ApiResponse<ClientViewModel>> GetByTaxNumberAsync(string taxNumber, CancellationToken cancellationToken = default)
        {
            return await _apiService.GetAsync<ClientViewModel>($"{BaseEndpoint}/tax-number/{taxNumber}", cancellationToken);
        }

        public async Task<ApiResponse<ClientViewModel>> GetByTcNumberAsync(string tcNumber, CancellationToken cancellationToken = default)
        {
            return await _apiService.GetAsync<ClientViewModel>($"{BaseEndpoint}/tc-number/{tcNumber}", cancellationToken);
        }

        public async Task<ApiResponse<ClientViewModel>> GetByCodeAsync(string clientCode, CancellationToken cancellationToken = default)
        {
            return await _apiService.GetAsync<ClientViewModel>($"{BaseEndpoint}/code/{clientCode}", cancellationToken);
        }
    }
}