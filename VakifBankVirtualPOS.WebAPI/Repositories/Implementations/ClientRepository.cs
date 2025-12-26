using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using VakifBankVirtualPOS.WebAPI.Data.Context;
using VakifBankVirtualPOS.WebAPI.Data.Entities;
using VakifBankVirtualPOS.WebAPI.Repositories.Interfaces;

namespace VakifBankVirtualPOS.WebAPI.Repositories.Implementations
{
    public class ClientRepository : IClientRepository

    {
        private readonly AppDbContext _context;
        private readonly ILogger<IClientRepository> _logger;

        public ClientRepository(
            AppDbContext context,
            ILogger<IClientRepository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IDT_CARI_KAYIT?> CreateClientAsync(IDT_CARI_KAYIT client, CancellationToken cancellationToken)
        {
            try
            {
                var parameters = new[]
        {
            new SqlParameter("@CARI_ISIM", client.CARI_ISIM),
            new SqlParameter("@VERGI_NO", client.VERGI_NUMARASI ?? (object)DBNull.Value),
            new SqlParameter("@VERGI_DAIRESI", client.VERGI_DAIRESI ?? (object)DBNull.Value),
            new SqlParameter("@TCKIMLIKNO", client.TCKIMLIKNO ?? (object)DBNull.Value),
            new SqlParameter("@ADRES", client.CARI_ADRES),
            new SqlParameter("@IL", client.CARI_IL),
            new SqlParameter("@ILCE", client.CARI_ILCE),
            new SqlParameter("@EPOSTA", client.EMAIL),
            new SqlParameter("@TEL", client.CARI_TEL),
            new SqlParameter("@SUBE_CARI_KOD", 1)
        };
                var result = await _context.Database.ExecuteSqlRawAsync(
              "EXEC [dbo].[IDP_CARI_KAYIT_I] @CARI_ISIM, @VERGI_NO, @VERGI_DAIRESI, @TCKIMLIKNO, @ADRES, @IL, @ILCE, @EPOSTA, @TEL, @SUBE_CARI_KOD",
              parameters,
              cancellationToken);

                if (result.ToString() == "-1")
                {
                    _logger.LogWarning("Cari kaydı oluşturulamadı. Aynı cari ismiyle kayıt mevcut. CariIsim: {CariIsim}", client.CARI_ISIM);
                    return null;
                }
                _logger.LogInformation("Cari kaydı oluşturuldu. CariIsim: {CariIsim}", client.CARI_ISIM);

                return client;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cari kaydı oluşturma hatası");
                throw;
            }
        }

        public async Task<IDT_CARI_HAREKET> CreateTransactionAsync(string orderId, CancellationToken cancellationToken)
        {
            try
            {
                var payment = await _context.IDT_VAKIFBANK_ODEME.FirstOrDefaultAsync(x => x.OrderId == orderId, cancellationToken);
                if (payment == null)
                {
                    _logger.LogError("Ödeme bulunamadı. OrderId: {OrderId}", orderId);
                }

                var newTranscation = new IDT_CARI_HAREKET()
                {
                    ACIKLAMA = payment.OrderId,
                    CARI_KODU = payment.ClientCode,
                    BELGE_NO = payment.DocumentNo ?? null,
                    TARIH = payment.CompletedAt.Value,
                    BORC = payment.Amount,
                    ALACAK = 0,
                    BAKIYE = null,
                    HAREKET_TIPI = "G",
                    KAYIT_KULL = null,
                    KAYIT_ZAMAN = DateTime.Now,
                    AKTARIM = 0
                };

                await _context.IDT_CARI_HAREKET.AddAsync(newTranscation, cancellationToken);

                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Cari hareket kaydı oluşturuldu. OrderId: {OrderId}, Id: {Id}",
                    newTranscation.ACIKLAMA, newTranscation.ID);

                return newTranscation;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cari hareket kaydı oluşturma hatası. OrderId: {OrderId}", orderId);
                throw;
            }
        }

        public async Task<IDT_CARI_KAYIT?> GetByTaxNumber(string taxNumber, CancellationToken cancellationToken)
        {
            try
            {
                return await _context.IDT_CARI_KAYIT.AsNoTracking().FirstOrDefaultAsync(x => x.VERGI_NUMARASI == taxNumber, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Vergi numarasına göre cari getirme işleminde hata");
                throw;
            }
        }

        public async Task<IDT_CARI_KAYIT?> GetByTcNumber(string tcNumber, CancellationToken cancellationToken)
        {
            try
            {
                return await _context.IDT_CARI_KAYIT.AsNoTracking().FirstOrDefaultAsync(x => x.TCKIMLIKNO == tcNumber, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "TC numarasına göre cari getirme işleminde hata");
                throw;
            }
        }
    }
}