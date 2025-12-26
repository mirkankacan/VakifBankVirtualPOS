using VakifBankVirtualPOS.WebAPI.Common;
using VakifBankVirtualPOS.WebAPI.Dtos.ClientDtos;
using VakifBankVirtualPOS.WebAPI.Dtos.HybsDtos;

namespace VakifBankVirtualPOS.WebAPI.Services.Interfaces
{
    public interface IClientService
    {
        Task<ServiceResult<ClientDto>> CheckByNoAsync(string no, CancellationToken cancellationToken);

        Task<ServiceResult<ClientDto>> GetByTaxNumberAsync(string taxNumber, CancellationToken cancellationToken);

        Task<ServiceResult<ClientDto>> GetByTcNumberAsync(string tcNumber, CancellationToken cancellationToken);

        Task<ServiceResult<ClientDto>> GetByCodeAsync(string clientCode, CancellationToken cancellationToken);

        Task<ServiceResult<DocumentCheckDto>> GetTransactionsByDocumentAsync(string documentNo, CancellationToken cancellationToken);
    }
}