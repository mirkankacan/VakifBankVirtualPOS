using System.Net;
using VakifBankVirtualPOS.WebAPI.Common;
using VakifBankVirtualPOS.WebAPI.Data.Entities;
using VakifBankVirtualPOS.WebAPI.Dtos;
using VakifBankVirtualPOS.WebAPI.Repositories.Interfaces;
using VakifBankVirtualPOS.WebAPI.Services.Interfaces;

namespace VakifBankVirtualPOS.WebAPI.Services.Implementations
{
    /// <summary>
    /// Müşteri işlemleri servisi
    /// </summary>
    public class ClientService : IClientService
    {
        private readonly IClientRepository _clientRepository;
        private readonly ILogger<ClientService> _logger;

        public ClientService(
            IClientRepository clientRepository,
            ILogger<ClientService> logger)
        {
            _clientRepository = clientRepository ?? throw new ArgumentNullException(nameof(clientRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Yeni müşteri oluşturur
        /// </summary>
        public async Task<ServiceResult<ClientResponseDto>> CreateClientAsync(
            CreateClientRequestDto request,
            CancellationToken cancellationToken)
        {
            if (request == null)
            {
                return ServiceResult<ClientResponseDto>.Error(
                    "Geçersiz İstek",
                    "Müşteri isteği boş olamaz",
                    HttpStatusCode.BadRequest);
            }

            try
            {
                _logger.LogInformation("Yeni müşteri oluşturuluyor. Cari İsim: {CariIsim}", request.CARI_ISIM);

                var client = new IDT_CARI_KAYIT
                {
                    CARI_ISIM = request.CARI_ISIM,
                    VERGI_NUMARASI = request.VERGI_NUMARASI,
                    VERGI_DAIRESI = request.VERGI_DAIRESI,
                    TCKIMLIKNO = request.TCKIMLIKNO,
                    CARI_ADRES = request.CARI_ADRES,
                    CARI_IL = request.CARI_IL,
                    CARI_ILCE = request.CARI_ILCE,
                    EMAIL = request.EMAIL,
                    CARI_TEL = request.CARI_TEL
                };

                var result = await _clientRepository.CreateClientAsync(client, cancellationToken);

                if (result == null)
                {
                    _logger.LogWarning("Müşteri oluşturulamadı. Aynı cari ismiyle kayıt mevcut. Cari İsim: {CariIsim}", request.CARI_ISIM);

                    return ServiceResult<ClientResponseDto>.Error(
                        "Conflict",
                        "Aynı cari ismiyle kayıt mevcut",
                        HttpStatusCode.Conflict);
                }

                var response = MapToResponseDto(result);
                var url = $"/api/clients/{result.ID}";

                _logger.LogInformation("Müşteri başarıyla oluşturuldu. ID: {Id}, Cari Kod: {CariKod}", result.ID, result.CARI_KOD);

                return ServiceResult<ClientResponseDto>.SuccessAsCreated(response, url);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Müşteri oluşturma hatası. Cari İsim: {CariIsim}", request.CARI_ISIM);

                return ServiceResult<ClientResponseDto>.Error(
                    "Internal Server Error",
                    $"Müşteri oluşturulurken bir hata oluştu: {ex.Message}",
                    HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Vergi numarasına göre müşteri getirir
        /// </summary>
        public async Task<ServiceResult<ClientResponseDto>> GetByTaxNumberAsync(
            string taxNumber,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(taxNumber))
            {
                return ServiceResult<ClientResponseDto>.Error(
                    "Geçersiz İstek",
                    "Vergi numarası boş olamaz",
                    HttpStatusCode.BadRequest);
            }

            try
            {
                _logger.LogInformation("Vergi numarasına göre müşteri getiriliyor. Vergi No: {TaxNumber}", taxNumber);

                var client = await _clientRepository.GetByTaxNumber(taxNumber, cancellationToken);

                if (client == null)
                {
                    _logger.LogWarning("Müşteri bulunamadı. Vergi No: {TaxNumber}", taxNumber);

                    return ServiceResult<ClientResponseDto>.Error(
                        "Not Found",
                        "Belirtilen vergi numarasına sahip müşteri bulunamadı",
                        HttpStatusCode.NotFound);
                }

                var response = MapToResponseDto(client);

                _logger.LogInformation("Müşteri bulundu. ID: {Id}, Cari Kod: {CariKod}", client.ID, client.CARI_KOD);

                return ServiceResult<ClientResponseDto>.SuccessAsOk(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Müşteri getirme hatası. Vergi No: {TaxNumber}", taxNumber);

                return ServiceResult<ClientResponseDto>.Error(
                    "Internal Server Error",
                    $"Müşteri getirilirken bir hata oluştu: {ex.Message}",
                    HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// TC kimlik numarasına göre müşteri getirir
        /// </summary>
        public async Task<ServiceResult<ClientResponseDto>> GetByTcNumberAsync(
            string tcNumber,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(tcNumber))
            {
                return ServiceResult<ClientResponseDto>.Error(
                    "Geçersiz İstek",
                    "TC kimlik numarası boş olamaz",
                    HttpStatusCode.BadRequest);
            }

            try
            {
                _logger.LogInformation("TC kimlik numarasına göre müşteri getiriliyor. TC No: {TcNumber}", tcNumber);

                var client = await _clientRepository.GetByTcNumber(tcNumber, cancellationToken);

                if (client == null)
                {
                    _logger.LogWarning("Müşteri bulunamadı. TC No: {TcNumber}", tcNumber);

                    return ServiceResult<ClientResponseDto>.Error(
                        "Not Found",
                        "Belirtilen TC kimlik numarasına sahip müşteri bulunamadı",
                        HttpStatusCode.NotFound);
                }

                var response = MapToResponseDto(client);

                _logger.LogInformation("Müşteri bulundu. ID: {Id}, Cari Kod: {CariKod}", client.ID, client.CARI_KOD);

                return ServiceResult<ClientResponseDto>.SuccessAsOk(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Müşteri getirme hatası. TC No: {TcNumber}", tcNumber);

                return ServiceResult<ClientResponseDto>.Error(
                    "Internal Server Error",
                    $"Müşteri getirilirken bir hata oluştu: {ex.Message}",
                    HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Entity'yi DTO'ya map eder
        /// </summary>
        private static ClientResponseDto MapToResponseDto(IDT_CARI_KAYIT client)
        {
            return new ClientResponseDto
            {
                ID = client.ID,
                CARI_KOD = client.CARI_KOD,
                CARI_ISIM = client.CARI_ISIM,
                VERGI_DAIRESI = client.VERGI_DAIRESI,
                VERGI_NUMARASI = client.VERGI_NUMARASI,
                TCKIMLIKNO = client.TCKIMLIKNO,
                CARI_ADRES = client.CARI_ADRES,
                CARI_IL = client.CARI_IL,
                CARI_ILCE = client.CARI_ILCE,
                EMAIL = client.EMAIL,
                CARI_TEL = client.CARI_TEL,
                BAKIYE = client.BAKIYE,
                SUBE_CARI_KOD = client.SUBE_CARI_KOD
            };
        }
    }
}

