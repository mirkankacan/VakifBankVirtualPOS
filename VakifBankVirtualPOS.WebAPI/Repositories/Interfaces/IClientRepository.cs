using VakifBankVirtualPOS.WebAPI.Data.Entities;

namespace VakifBankVirtualPOS.WebAPI.Repositories.Interfaces
{
    public interface IClientRepository
    {
        Task<IDT_CARI_HAREKET?> CreateTransactionAsync(string orderId, CancellationToken cancellationToken);

        Task<IDT_CARI_KAYIT?> GetByTaxNumber(string taxNumber, CancellationToken cancellationToken);

        Task<IDT_CARI_KAYIT?> GetByTcNumber(string tcNumber, CancellationToken cancellationToken);

        Task<IDT_CARI_KAYIT> CreateClientAsync(IDT_CARI_KAYIT client, CancellationToken cancellationToken);
    }
}