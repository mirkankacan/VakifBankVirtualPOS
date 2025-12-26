using Microsoft.EntityFrameworkCore;
using VakifBankVirtualPOS.WebAPI.Data.Context;
using VakifBankVirtualPOS.WebAPI.Data.Entities;
using VakifBankVirtualPOS.WebAPI.Repositories.Interfaces;

namespace VakifBankVirtualPOS.WebAPI.Repositories.Implementations
{
    /// <summary>
    /// Payment repository implementation
    /// </summary>
    public class PaymentRepository : IPaymentRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<PaymentRepository> _logger;

        public PaymentRepository(
            AppDbContext context,
            ILogger<PaymentRepository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Yeni ödeme kaydı oluşturur
        /// </summary>
        public async Task<IDT_VAKIFBANK_ODEME> CreateAsync(IDT_VAKIFBANK_ODEME payment, CancellationToken cancellationToken)
        {
            try
            {
                payment.CreatedAt = DateTime.Now;

                await _context.IDT_VAKIFBANK_ODEME.AddAsync(payment, cancellationToken);

                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Ödeme kaydı oluşturuldu. OrderId: {OrderId}, Id: {Id}",
                    payment.OrderId, payment.Id);

                return payment;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ödeme kaydı oluşturma hatası. OrderId: {OrderId}", payment.OrderId);
                throw;
            }
        }

        /// <summary>
        /// OrderId ile ödeme kaydını getirir
        /// </summary>
        public async Task<IDT_VAKIFBANK_ODEME?> GetByOrderIdAsync(string orderId, CancellationToken cancellationToken)
        {
            try
            {
                return await _context.IDT_VAKIFBANK_ODEME
                    .FirstOrDefaultAsync(p => p.OrderId == orderId, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ödeme kaydı getirme hatası. OrderId: {OrderId}", orderId);
                throw;
            }
        }

        /// <summary>
        /// OrderId ile ödeme kaydını günceller
        /// </summary>
        public async Task<IDT_VAKIFBANK_ODEME?> UpdateByOrderIdAsync(
            string orderId,
            string status,
            CancellationToken cancellationToken,
            string? transactionId = null,
            string? authCode = null,
            string? errorCode = null,
            string? errorMessage = null)
        {
            try
            {
                var payment = await GetByOrderIdAsync(orderId, cancellationToken);

                if (payment == null)
                {
                    _logger.LogWarning("Güncellenecek ödeme kaydı bulunamadı. OrderId: {OrderId}", orderId);
                    return null;
                }

                // Değerleri güncelle
                payment.Status = status;
                payment.UpdatedAt = DateTime.Now;

                if (!string.IsNullOrEmpty(transactionId))
                    payment.TransactionId = transactionId;

                if (!string.IsNullOrEmpty(authCode))
                    payment.AuthCode = authCode;

                if (!string.IsNullOrEmpty(errorCode))
                    payment.ErrorCode = errorCode;

                if (!string.IsNullOrEmpty(errorMessage))
                    payment.ErrorMessage = errorMessage;

                // Başarılı veya başarısız durumda CompletedAt'i set et
                if (status == "Success" || status == "Failed")
                {
                    payment.CompletedAt = DateTime.Now;
                }

                _context.IDT_VAKIFBANK_ODEME.Update(payment);
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Ödeme kaydı güncellendi. OrderId: {OrderId}, Status: {Status}",
                    orderId, status);

                return payment;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ödeme kaydı güncelleme hatası. OrderId: {OrderId}", orderId);
                throw;
            }
        }
    }
}