using VakifBankVirtualPOS.WebUI.Common;
using VakifBankVirtualPOS.WebUI.Models.ClientViewModels;

namespace VakifBankVirtualPOS.WebUI.Services.Interfaces
{
    public interface IClientApiService
    {
        Task<ApiResponse<ClientViewModel>> CheckByNoAsync(string no, CancellationToken cancellationToken = default);

        Task<ApiResponse<ClientViewModel>> GetByTaxNumberAsync(string taxNumber, CancellationToken cancellationToken = default);

        Task<ApiResponse<ClientViewModel>> GetByTcNumberAsync(string tcNumber, CancellationToken cancellationToken = default);
    }
}