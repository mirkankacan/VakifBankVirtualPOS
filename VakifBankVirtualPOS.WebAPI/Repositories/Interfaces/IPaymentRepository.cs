using VakifBankVirtualPOS.WebAPI.Data.Entities;

namespace VakifBankVirtualPOS.WebAPI.Repositories.Interfaces
{
    /// <summary>
    /// Payment repository interface
    /// </summary>
    public interface IPaymentRepository
    {
        /// <summary>
        /// Yeni ödeme kaydı oluşturur
        /// </summary>
        Task<IDT_VAKIFBANK_ODEME> CreateAsync(IDT_VAKIFBANK_ODEME payment, CancellationToken cancellationToken);

        /// <summary>
        /// OrderId ile ödeme kaydını getirir
        /// </summary>
        Task<IDT_VAKIFBANK_ODEME?> GetByOrderIdAsync(string orderId, CancellationToken cancellationToken);

        Task<IDT_VAKIFBANK_ODEME?> UpdateByOrderIdAsync(string orderId, string status, CancellationToken cancellationToken, string? transactionId = null, string? authCode = null, string? errorCode = null, string? errorMessage = null);
    }
}