using Mapster;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Net;
using VakifBankVirtualPOS.WebAPI.Common;
using VakifBankVirtualPOS.WebAPI.Data.Context;
using VakifBankVirtualPOS.WebAPI.Dtos.ClientDtos;
using VakifBankVirtualPOS.WebAPI.Dtos.HybsDtos;
using VakifBankVirtualPOS.WebAPI.Services.Interfaces;

namespace VakifBankVirtualPOS.WebAPI.Services.Implementations
{
    public class ClientService : IClientService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<IClientService> _logger;
        private readonly IHybsService _hybsService;

        public ClientService(
            AppDbContext context,
            ILogger<IClientService> logger,
            IHybsService hybsService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _hybsService = hybsService ?? throw new ArgumentNullException(nameof(hybsService));
        }

        public async Task<ServiceResult<ClientDto>> CheckByNoAsync(string no, CancellationToken cancellationToken)
        {
            try
            {
                var client = await CheckByNoInternalAsync(no, cancellationToken);

                if (client != null)
                {
                    return ServiceResult<ClientDto>.SuccessAsOk(client.Adapt<ClientDto>());
                }

                var clientDetails = await _hybsService.GetClientDetailByNoAsync(no, cancellationToken);
                if (clientDetails == null)
                {
                    return ServiceResult<ClientDto>.Error("HYBS sisteminde cari bilgileri bulunamadı", HttpStatusCode.NotFound);
                }
                var newClient = await CreateClientAsync(clientDetails, cancellationToken);
                if (newClient == null)
                {
                    return ServiceResult<ClientDto>.Error(
                        "Cari Oluşturma Hatası",
                        "Cari kaydı oluşturulamadı. Aynı cari ismiyle kayıt mevcut olabilir.",
                        HttpStatusCode.InternalServerError);
                }
                return ServiceResult<ClientDto>.SuccessAsOk(newClient.Adapt<ClientDto>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Numara ile cari kontrol işleminde hata. No: {No}", no);
                throw;
            }
        }

        public async Task<ServiceResult<ClientDto>> GetByCodeAsync(string clientCode, CancellationToken cancellationToken)
        {
            try
            {
                var client = await _context.IDT_CARI_KAYIT
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.CARI_KOD == clientCode, cancellationToken);

                if (client == null)
                {
                    return ServiceResult<ClientDto>.Error(
                        "Cari bulunamadı",
                        $"{clientCode} kodlu cari bulunamadı",
                        HttpStatusCode.NotFound);
                }

                return ServiceResult<ClientDto>.SuccessAsOk(client.Adapt<ClientDto>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cari koda göre cari getirme işleminde hata. ClientCode: {ClientCode}", clientCode);
                throw;
            }
        }

        public async Task<ServiceResult<ClientDto>> GetByTaxNumberAsync(string taxNumber, CancellationToken cancellationToken)
        {
            try
            {
                var client = await _context.IDT_CARI_KAYIT
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.VERGI_NUMARASI == taxNumber, cancellationToken);

                if (client == null)
                {
                    return ServiceResult<ClientDto>.Error(
                        "Cari Bulunamadı",
                        $"{taxNumber} vergi numaralı cari bulunamadı",
                        HttpStatusCode.NotFound);
                }

                return ServiceResult<ClientDto>.SuccessAsOk(client.Adapt<ClientDto>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Vergi numarasına göre cari getirme işleminde hata. TaxNumber: {TaxNumber}", taxNumber);
                throw;
            }
        }

        public async Task<ServiceResult<ClientDto>> GetByTcNumberAsync(string tcNumber, CancellationToken cancellationToken)
        {
            try
            {
                var client = await _context.IDT_CARI_KAYIT
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.TCKIMLIKNO == tcNumber, cancellationToken);

                if (client == null)
                {
                    return ServiceResult<ClientDto>.Error(
                        "Cari Bulunamadı",
                        $"{tcNumber} TC kimlik numaralı cari bulunamadı",
                        HttpStatusCode.NotFound);
                }

                return ServiceResult<ClientDto>.SuccessAsOk(client.Adapt<ClientDto>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "TC numarasına göre cari getirme işleminde hata. TcNumber: {TcNumber}", tcNumber);
                throw;
            }
        }

        public async Task<ServiceResult<DocumentCheckDto>> GetTransactionsByDocumentAsync(string documentNo, CancellationToken cancellationToken)
        {
            try
            {
                var clientTransactions = await _context.IDT_CARI_HAREKET
                    .AsNoTracking()
                    .Where(x => x.BELGE_NO == documentNo)
                    .ToListAsync(cancellationToken);

                if (clientTransactions == null || !clientTransactions.Any())
                {
                    return ServiceResult<DocumentCheckDto>.SuccessAsOk(new DocumentCheckDto
                    {
                        IsDocumentExist = false,
                        DocumentProcessCount = 0,
                        DocumentNo = documentNo,
                        DocumentProcessList = new List<DocumentProcessList>()
                    });
                }

                return ServiceResult<DocumentCheckDto>.SuccessAsOk(clientTransactions.Adapt<DocumentCheckDto>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Doküman numarasına göre cari hareketleri getirme işleminde hata. DocumentNo: {DocumentNo}", documentNo);
                throw;
            }
        }

        #region Private Helper Methods

        private async Task<Data.Entities.IDT_CARI_KAYIT?> CheckByNoInternalAsync(string no, CancellationToken cancellationToken)
        {
            try
            {
                Data.Entities.IDT_CARI_KAYIT? client = null;

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

        private async Task<Data.Entities.IDT_CARI_KAYIT?> CreateClientAsync(ClientDetailDto client, CancellationToken cancellationToken)
        {
            try
            {
                var parameters = new[]
                {
                    new SqlParameter("@CARI_ISIM", client.FirmaAdi),
                    new SqlParameter("@VERGI_NO", client.VergiNo.Length == 10 ? client.VergiNo : (object)DBNull.Value),
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
                Data.Entities.IDT_CARI_KAYIT? createdClient = null;

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

        #endregion
    }
}