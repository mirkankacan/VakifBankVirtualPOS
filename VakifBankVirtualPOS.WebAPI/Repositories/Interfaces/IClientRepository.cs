using VakifBankVirtualPOS.WebAPI.Data.Entities;
using VakifBankVirtualPOS.WebAPI.Dtos.HybsDtos;

namespace VakifBankVirtualPOS.WebAPI.Repositories.Interfaces
{
    public interface IClientRepository
    {
        Task<IDT_CARI_HAREKET?> CreateTransactionAsync(string orderId, CancellationToken cancellationToken);

        Task<List<IDT_CARI_HAREKET>?> GetTransactionsByDocumentAsync(string documentNo, CancellationToken cancellationToken);

        Task<IDT_CARI_KAYIT?> GetByTaxNumberAsync(string taxNumber, CancellationToken cancellationToken);

        Task<IDT_CARI_KAYIT?> CheckByNoAsync(string no, CancellationToken cancellationToken);

        Task<IDT_CARI_KAYIT?> GetByTcNumberAsync(string tcNumber, CancellationToken cancellationToken);

        Task<IDT_CARI_KAYIT?> GetByCodeAsync(string clientCode, CancellationToken cancellationToken);

        Task<IDT_CARI_KAYIT> CreateClientAsync(ClientDetailDto client, CancellationToken cancellationToken);
    }
}