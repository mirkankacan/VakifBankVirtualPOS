using VakifBankVirtualPOS.WebAPI.Dtos.HybsDtos;

namespace VakifBankVirtualPOS.WebAPI.Services.Interfaces
{
    public interface IHybsService
    {
        Task<ClientDetailDto?> GetClientDetailByNoAsync(string no, CancellationToken cancellationToken);

        Task<string> GetHybsTokenAsync();
    }
}