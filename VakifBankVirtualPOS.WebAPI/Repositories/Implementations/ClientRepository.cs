using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using VakifBankVirtualPOS.WebAPI.Data.Context;
using VakifBankVirtualPOS.WebAPI.Data.Entities;
using VakifBankVirtualPOS.WebAPI.Dtos.HybsDtos;
using VakifBankVirtualPOS.WebAPI.Repositories.Interfaces;

namespace VakifBankVirtualPOS.WebAPI.Repositories.Implementations
{
    public class ClientRepository : IClientRepository

    {
        private readonly AppDbContext _context;
        private readonly ILogger<IClientRepository> _logger;

        public ClientRepository(
            AppDbContext context,
            ILogger<IClientRepository> logger
            )
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IDT_CARI_KAYIT?> CheckByNoAsync(string no, CancellationToken cancellationToken)
        {
            try
            {
                IDT_CARI_KAYIT? client = null;

                switch (no.Length)
                {
                    case 10:
                        client = await _context.IDT_CARI_KAYIT
                            .AsNoTracking()
                            .FirstOrDefaultAsync(x => x.VERGI_NUMARASI == no, cancellationToken);
                        break;

                    case 11:
                        client = await _context.IDT_CARI_KAYIT
                            .AsNoTracking()
                            .FirstOrDefaultAsync(x => x.TCKIMLIKNO == no, cancellationToken);
                        break;

                    default:
                        break;
                }
                return client;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Numara ile cari arama işleminde hata");
                throw;
            }
        }

        public async Task<List<IDT_CARI_HAREKET>?> GetTransactionsByDocumentAsync(string documentNo, CancellationToken cancellationToken)
        {
            try
            {
                return await _context.IDT_CARI_HAREKET.AsNoTracking().Where(x => x.BELGE_NO == documentNo).ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Doküman numarasına göre cari hareket getirme işleminde hata");
                throw;
            }
        }

        public async Task<IDT_CARI_KAYIT?> CreateClientAsync(ClientDetailDto client, CancellationToken cancellationToken)
        {
            try
            {
                var parameters = new[]
        {
            new SqlParameter("@CARI_ISIM", client.FirmaAdi),
            new SqlParameter("@VERGI_NO", client.VergiNo.Length == 10 ? client.VergiNo :(object)DBNull.Value),
            new SqlParameter("@VERGI_DAIRESI", client.VergiDairesi ?? (object)DBNull.Value),
            new SqlParameter("@TCKIMLIKNO", client.VergiNo.Length == 11 ? client.VergiNo : (object)DBNull.Value),
            new SqlParameter("@ADRES", client.Adres ?? (object)DBNull.Value),
            new SqlParameter("@IL", client.IlAdi ?? (object)DBNull.Value),
            new SqlParameter("@ILCE", client.IlceAdi ?? (object)DBNull.Value),
            new SqlParameter("@EPOSTA", client.Eposta ?? (object)DBNull.Value),
            new SqlParameter("@TEL", client.GSM1 ?? (object)DBNull.Value),
            new SqlParameter("@SUBE_CARI_KOD", 1)
        };
                var result = await _context.Database.ExecuteSqlRawAsync(
              "EXEC [dbo].[IDP_CARI_KAYIT_I] @CARI_ISIM, @VERGI_NO, @VERGI_DAIRESI, @TCKIMLIKNO, @ADRES, @IL, @ILCE, @EPOSTA, @TEL, @SUBE_CARI_KOD",
              parameters,
              cancellationToken);

                if (result.ToString() == "-1")
                {
                    _logger.LogWarning("Cari kaydı oluşturulamadı. Aynı cari ismiyle kayıt mevcut. CariIsim: {CariIsim}", client.FirmaAdi);
                    return null;
                }
                _logger.LogInformation("Cari kaydı oluşturuldu. CariIsim: {CariIsim}", client.FirmaAdi);

                // Stored procedure'den dönen sonucu kullanarak kaydı tekrar çek
                IDT_CARI_KAYIT? createdClient = null;

                switch (client.VergiNo.Length)
                {
                    case 10:
                        createdClient = await _context.IDT_CARI_KAYIT
                            .AsNoTracking()
                            .FirstOrDefaultAsync(x => x.VERGI_NUMARASI == client.VergiNo, cancellationToken);
                        break;

                    case 11:
                        createdClient = await _context.IDT_CARI_KAYIT
                            .AsNoTracking()
                            .FirstOrDefaultAsync(x => x.TCKIMLIKNO == client.VergiNo, cancellationToken);
                        break;

                    default:
                        break;
                }
                return createdClient;
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

        public async Task<IDT_CARI_KAYIT?> GetByCodeAsync(string clientCode, CancellationToken cancellationToken)
        {
            try
            {
                return await _context.IDT_CARI_KAYIT.AsNoTracking().FirstOrDefaultAsync(x => x.CARI_KOD == clientCode, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cari koda göre cari getirme işleminde hata");
                throw;
            }
        }

        public async Task<IDT_CARI_KAYIT?> GetByTaxNumberAsync(string taxNumber, CancellationToken cancellationToken)
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

        public async Task<IDT_CARI_KAYIT?> GetByTcNumberAsync(string tcNumber, CancellationToken cancellationToken)
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