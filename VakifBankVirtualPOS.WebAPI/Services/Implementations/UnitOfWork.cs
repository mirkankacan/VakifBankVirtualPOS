using Microsoft.EntityFrameworkCore.Storage;
using VakifBankVirtualPOS.WebAPI.Data.Context;
using VakifBankVirtualPOS.WebAPI.Services.Interfaces;

namespace VakifBankVirtualPOS.WebAPI.Services.Implementations
{
    /// <summary>
    /// Unit of Work pattern implementation for managing database transactions
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private readonly ILogger<UnitOfWork> _logger;
        private IDbContextTransaction? _transaction;

        public UnitOfWork(
            AppDbContext context,
            ILogger<UnitOfWork> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Asynchronously saves all changes made in this unit of work to the database
        /// </summary>
        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _context.SaveChangesAsync(cancellationToken);
                _logger.LogDebug("Değişiklikler veritabanına kaydedildi. Etkilenen satır sayısı: {Count}", result);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Değişiklikler kaydedilirken hata oluştu");
                throw;
            }
        }

        /// <summary>
        /// Begins a new database transaction
        /// </summary>
        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                if (_transaction != null)
                {
                    _logger.LogWarning("Zaten aktif bir transaction mevcut. Yeni transaction başlatılamadı.");
                    return;
                }

                _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
                _logger.LogDebug("Yeni database transaction başlatıldı");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Transaction başlatılırken hata oluştu");
                throw;
            }
        }

        /// <summary>
        /// Commits the current database transaction
        /// </summary>
        public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                if (_transaction == null)
                {
                    _logger.LogWarning("Commit edilecek aktif transaction bulunamadı");
                    return;
                }

                await _transaction.CommitAsync(cancellationToken);
                _logger.LogDebug("Database transaction commit edildi");

                await _transaction.DisposeAsync();
                _transaction = null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Transaction commit edilirken hata oluştu");
                await RollbackTransactionAsync(cancellationToken);
                throw;
            }
        }

        /// <summary>
        /// Rolls back the current database transaction
        /// </summary>
        public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                if (_transaction == null)
                {
                    _logger.LogWarning("Rollback edilecek aktif transaction bulunamadı");
                    return;
                }

                await _transaction.RollbackAsync(cancellationToken);
                _logger.LogWarning("Database transaction rollback edildi");

                await _transaction.DisposeAsync();
                _transaction = null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Transaction rollback edilirken hata oluştu");
                throw;
            }
        }

        /// <summary>
        /// Disposes the unit of work and releases resources
        /// </summary>
        public void Dispose()
        {
            try
            {
                if (_transaction != null)
                {
                    _transaction.Dispose();
                    _transaction = null;
                    _logger.LogDebug("UnitOfWork dispose edildi ve transaction temizlendi");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UnitOfWork dispose edilirken hata oluştu");
            }
        }
    }
}

