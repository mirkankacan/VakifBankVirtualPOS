using Microsoft.Extensions.Caching.Distributed;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using VakifBankPayment.WebAPI.Helpers;
using VakifBankVirtualPOS.WebAPI.Common;
using VakifBankVirtualPOS.WebAPI.Data.Entities;
using VakifBankVirtualPOS.WebAPI.Dtos;
using VakifBankVirtualPOS.WebAPI.Options;
using VakifBankVirtualPOS.WebAPI.Repositories.Interfaces;
using VakifBankVirtualPOS.WebAPI.Services.Interfaces;

namespace VakifBankPayment.Services.Implementations
{
    /// <summary>
    /// VakıfBank Sanal POS ödeme servisi
    /// </summary>
    public class VakifBankService : IVakifBankService
    {
        private readonly VakifBankOptions _options;
        private readonly IHttpClientService _httpClientService;
        private readonly IXmlService _xmlService;
        private readonly IDistributedCache _cache;
        private readonly ILogger<VakifBankService> _logger;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IClientRepository _clientRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public VakifBankService(
            VakifBankOptions options,
            IHttpClientService httpClientService,
            IXmlService xmlService,
            IDistributedCache cache,
            ILogger<VakifBankService> logger,
            IPaymentRepository paymentRepository,
            IClientRepository clientRepository,
            IHttpContextAccessor httpContextAccessor)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _httpClientService = httpClientService ?? throw new ArgumentNullException(nameof(httpClientService));
            _xmlService = xmlService ?? throw new ArgumentNullException(nameof(xmlService));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
            _clientRepository = clientRepository ?? throw new ArgumentNullException(nameof(clientRepository));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        /// <summary>
        /// 3D Secure işlemini başlatır (Enrollment)
        /// </summary>
        public async Task<ServiceResult<EnrollmentResponseDto>> InitiateThreeDSecureAsync(
            PaymentInitiateRequestDto request,
            CancellationToken cancellationToken)
        {
            if (request == null)
            {
                return ServiceResult<EnrollmentResponseDto>.Error(
                    "Geçersiz İstek",
                    "Ödeme isteği boş olamaz",
                    HttpStatusCode.BadRequest);
            }

            try
            {
                var clientIp = GetClientIp();
                _logger.LogInformation("3D Secure başlatılıyor. Tutar: {Amount}, IP: {ClientIp}",
                    request.Amount, clientIp);

                // 1. Kart numarası validasyonu
                if (!CardHelper.IsValidCardNumber(request.CardNumber))
                {
                    _logger.LogWarning("Geçersiz kart numarası: {MaskedCard}",
                        CardHelper.MaskCardNumber(request.CardNumber));

                    return ServiceResult<EnrollmentResponseDto>.Error(
                        "Geçersiz Kart",
                        "Kart numarası geçersiz (Luhn kontrolü başarısız)",
                        HttpStatusCode.BadRequest);
                }

                // 2. Kart kuruluşunu bul
                var brandName = CardHelper.GetBrandName(request.CardNumber);
                _logger.LogInformation("Kart kuruluşu: {Brand}",
                    CardHelper.GetBrandDisplayName(request.CardNumber));

                // 3. OrderId oluştur
                var orderId = GenerateOrderId();

                // 4. Kart bilgilerini temizle
                var cleanCardNumber = request.CardNumber.Replace(" ", "").Replace("-", "");

                // 5. Ödeme bilgilerini cache'e kaydet
                var paymentData = new
                {
                    Cvv = request.Cvv,
                    CardHolderName = request.CardHolderName,
                    Amount = request.Amount,
                    CardNumber = cleanCardNumber,
                    ExpiryDate = request.ExpiryDate,
                    BrandName = brandName,
                    ClientIp = clientIp,
                    CreatedAt = DateTime.UtcNow
                };

                var cacheOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_options.CacheTimeoutMinutes)
                };

                await _cache.SetStringAsync(
                    $"payment:{orderId}",
                    JsonSerializer.Serialize(paymentData),
                    cacheOptions);

                _logger.LogInformation("Ödeme bilgileri cache'e kaydedildi. OrderId: {OrderId}", orderId);

                // 6. Enrollment isteği hazırla
                var formData = new Dictionary<string, string>
                {
                    { "MerchantId", _options.MerchantId },
                    { "MerchantPassword", _options.MerchantPassword },
                    { "VerifyEnrollmentRequestId", Guid.NewGuid().ToString() },
                    { "Pan", cleanCardNumber },
                    { "ExpiryDate", CardHelper.ConvertExpiryDateToYYMM(request.ExpiryDate)},
                    { "PurchaseAmount", CardHelper.FormatAmount(request.Amount) },
                    { "Currency", _options.Currency },
                    { "BrandName", brandName },
                    { "SuccessUrl", _options.SuccessUrl },
                    { "FailureUrl", _options.FailureUrl }
                };

                // 7. HTTP POST isteği gönder
                _logger.LogDebug("Enrollment isteği gönderiliyor. URL: {Url}", _options.EnrollmentUrl);

                var responseXml = await _httpClientService.PostFormDataAsync(
                    _options.EnrollmentUrl,
                    formData,
                    cancellationToken);

                // 8. XML cevabını parse et
                var enrollmentResponse = ParseEnrollmentResponse(responseXml);

                if (enrollmentResponse.Status == "Error")
                {
                    _logger.LogError("Enrollment hatası: {Message}", enrollmentResponse.Message);

                    return ServiceResult<EnrollmentResponseDto>.Error(
                        "3D Secure Hatası",
                        enrollmentResponse.Message,
                        HttpStatusCode.BadRequest);
                }

                // 9. OrderId'yi response'a ekle
                enrollmentResponse.OrderId = orderId;

                var payment = await _paymentRepository.CreateAsync(new IDT_VAKIFBANK_ODEME
                {
                    OrderId = orderId,
                    Amount = request.Amount,
                    Currency = _options.Currency,
                    MaskedCardNumber = CardHelper.MaskCardNumber(cleanCardNumber),
                    CardHolderName = request.CardHolderName,
                    CardBrand = CardHelper.GetBrandDisplayName(cleanCardNumber),
                    Status = "Pending",
                    ThreeDSecureStatus = enrollmentResponse.Status,
                    ClientIp = clientIp,
                    ClientCode = request.ClientCode,
                    DocumentNo = request.DocumentNo ?? null
                }, cancellationToken);

                _logger.LogInformation("3D Secure başarıyla başlatıldı. OrderId: {OrderId}, Status: {Status}",
                    orderId, enrollmentResponse.Status);

                return ServiceResult<EnrollmentResponseDto>.SuccessAsOk(enrollmentResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "3D Secure başlatma hatası");

                throw;
            }
        }

        /// <summary>
        /// 3D Secure doğrulama sonrası ödemeyi tamamlar
        /// </summary>
        public async Task<ServiceResult<PaymentResultDto>> CompletePaymentAsync(
            ThreeDCallbackDto callback,
            CancellationToken cancellationToken)
        {
            if (callback == null)
            {
                return ServiceResult<PaymentResultDto>.Error(
                    "Geçersiz İstek",
                    "Callback verileri boş olamaz",
                    HttpStatusCode.BadRequest);
            }

            try
            {
                var clientIp = GetClientIp();
                _logger.LogInformation("3D Secure callback alındı. Status: {Status}, IP: {ClientIp}",
                    callback.Status, clientIp);

                // 1. 3D Secure status kontrolü
                if (callback.Status != "Y")
                {
                    var statusMessage = GetMdStatusMessage(callback.Status);
                    _logger.LogWarning("3D Secure doğrulama başarısız. Status: {Status}", callback.Status);

                    return ServiceResult<PaymentResultDto>.Error(
                        "3D Secure Başarısız",
                        statusMessage,
                        HttpStatusCode.BadRequest);
                }

                // 2. MD verisini parse et
                var mdData = ParseMD(callback.MD);

                if (!mdData.ContainsKey("OrderId"))
                {
                    _logger.LogError("MD verisinde OrderId bulunamadı");

                    return ServiceResult<PaymentResultDto>.Error(
                        "Geçersiz MD",
                        "MD verisinde OrderId bulunamadı",
                        HttpStatusCode.BadRequest);
                }

                var orderId = mdData["OrderId"];
                _logger.LogInformation("Ödeme tamamlanıyor. OrderId: {OrderId}", orderId);

                // 3. Cache'ten ödeme bilgilerini al
                var cachedJson = await _cache.GetStringAsync($"payment:{orderId}");

                if (string.IsNullOrEmpty(cachedJson))
                {
                    _logger.LogWarning("Cache'te ödeme bilgisi bulunamadı. OrderId: {OrderId}", orderId);

                    return ServiceResult<PaymentResultDto>.Error(
                        "Session Timeout",
                        "Ödeme oturumu zaman aşımına uğradı. Lütfen işlemi yeniden başlatın.",
                        HttpStatusCode.BadRequest);
                }

                var paymentData = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(cachedJson);

                // 4. VPOS provizyon isteği hazırla
                var vposRequest = new VposRequestDto
                {
                    MerchantId = _options.MerchantId,
                    Password = _options.MerchantPassword,
                    TransactionType = _options.TransactionType,
                    TransactionId = Guid.NewGuid().ToString().ToUpper(),
                    TerminalNo = _options.TerminalNo,
                    CurrencyAmount = CardHelper.FormatAmount(paymentData["Amount"].GetDecimal()),
                    CurrencyCode = _options.Currency,
                    Pan = paymentData["CardNumber"].GetString(),
                    Expiry = paymentData["ExpiryDate"].GetString(),
                    Cvv = paymentData["Cvv"].GetString(),
                    ClientIp = clientIp,
                    OrderId = orderId,
                    TransactionDeviceSource = _options.TransactionDeviceSource,

                    // 3D Secure bilgileri
                    ECI = callback.Eci,
                    CAVV = callback.Cavv,
                    MpiTransactionId = callback.MpiTransactionId ?? callback.VerifyEnrollmentRequestId
                };

                // 6. XML oluştur ve VPOS'a gönder
                _logger.LogDebug("VPOS provizyon isteği gönderiliyor. URL: {Url}", _options.VposUrl);

                var xmlRequest = _xmlService.SerializeToXml(vposRequest);
                var xmlResponse = await _httpClientService.PostXmlAsync(_options.VposUrl, xmlRequest, cancellationToken);

                // 7. Cevabı parse et
                var paymentResult = ParseVposResponse(xmlResponse);

                // 8. Cache'i temizle
                await _cache.RemoveAsync($"payment:{orderId}");
                _logger.LogInformation("Cache temizlendi. OrderId: {OrderId}", orderId);

                // 9. Sonucu kontrol et
                if (!paymentResult.IsSuccess)
                {
                    await _paymentRepository.UpdateByOrderIdAsync(
                      orderId: orderId,
                      status: "Failed",
                      cancellationToken: cancellationToken,
                      transactionId: paymentResult.TransactionId,
                      errorCode: paymentResult.ErrorCode,
                      errorMessage: paymentResult.Message
                  );
                    _logger.LogWarning("Ödeme başarısız. OrderId: {OrderId}, ErrorCode: {ErrorCode}, Message: {Message}",
                        orderId, paymentResult.ErrorCode, paymentResult.Message);

                    return ServiceResult<PaymentResultDto>.Error(
                        "Ödeme Başarısız",
                        paymentResult.Message,
                        HttpStatusCode.BadRequest);
                }
                await _paymentRepository.UpdateByOrderIdAsync(
                    orderId: orderId,
                    status: "Success",
                    cancellationToken: cancellationToken,
                    transactionId: paymentResult.TransactionId,
                    authCode: paymentResult.AuthCode
                );

                await _clientRepository.CreateTransactionAsync(orderId, cancellationToken);
                _logger.LogInformation("Ödeme başarılı. OrderId: {OrderId}, TransactionId: {TransactionId}, AuthCode: {AuthCode}",
                    orderId, paymentResult.TransactionId, paymentResult.AuthCode);

                return ServiceResult<PaymentResultDto>.SuccessAsOk(paymentResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ödeme tamamlama hatası");

                throw;
            }
        }

        #region Private Helper Methods

        /// <summary>
        /// OrderId oluşturur
        /// </summary>
        private string GenerateOrderId()
        {
            return $"EGESEHIR-{DateTime.Now:yyyyMMddHHmmss}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
        }

        /// <summary>
        /// Enrollment XML cevabını parse eder
        /// </summary>
        private EnrollmentResponseDto ParseEnrollmentResponse(string xmlResponse)
        {
            try
            {
                var result = new EnrollmentResponseDto
                {
                    Status = _xmlService.GetElementValue(xmlResponse, "Status"),
                    Message = _xmlService.GetElementValue(xmlResponse, "Message"),
                    MessageErrorCode = _xmlService.GetElementValue(xmlResponse, "MessageErrorCode")
                };

                if (result.Status == "Y")
                {
                    result.ACSUrl = _xmlService.GetElementValue(xmlResponse, "ACSUrl");
                    result.PAReq = _xmlService.GetElementValue(xmlResponse, "PAReq");
                    result.TermUrl = _xmlService.GetElementValue(xmlResponse, "TermUrl");
                    result.MD = _xmlService.GetElementValue(xmlResponse, "MD");
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Enrollment XML parse hatası");

                throw;
            }
        }

        /// <summary>
        /// VPOS XML cevabını parse eder
        /// </summary>
        private PaymentResultDto ParseVposResponse(string xmlResponse)
        {
            try
            {
                var resultCode = _xmlService.GetElementValue(xmlResponse, "ResultCode");
                var resultDetail = _xmlService.GetElementValue(xmlResponse, "ResultDetail");
                var transactionId = _xmlService.GetElementValue(xmlResponse, "TransactionId");
                var authCode = _xmlService.GetElementValue(xmlResponse, "AuthCode");
                var orderId = _xmlService.GetElementValue(xmlResponse, "OrderId");

                var isSuccess = resultCode == "0000";

                return new PaymentResultDto
                {
                    IsSuccess = isSuccess,
                    Message = isSuccess ? "Ödeme başarılı" : resultDetail,
                    TransactionId = transactionId,
                    AuthCode = authCode,
                    OrderId = orderId,
                    ErrorCode = resultCode
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "VPOS XML parse hatası");

                throw;
            }
        }

        /// <summary>
        /// MD verisini parse eder
        /// </summary>
        private Dictionary<string, string> ParseMD(string md)
        {
            var mdData = new Dictionary<string, string>();

            try
            {
                var decodedMd = md;

                // Base64 decode dene
                try
                {
                    var base64Bytes = Convert.FromBase64String(md);
                    decodedMd = Encoding.UTF8.GetString(base64Bytes);
                }
                catch
                {
                    // Base64 değilse direkt kullan
                }

                // Key-value parse et
                var pairs = decodedMd.Split('&');
                foreach (var pair in pairs)
                {
                    var keyValue = pair.Split('=');
                    if (keyValue.Length == 2)
                    {
                        mdData[keyValue[0]] = keyValue[1];
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MD parse hatası");
            }

            return mdData;
        }

        /// <summary>
        /// MD Status mesajını döndürür
        /// </summary>
        private string GetMdStatusMessage(string status)
        {
            return status switch
            {
                "Y" => "3D Secure doğrulama başarılı",
                "N" => "Kart 3D Secure programına kayıtlı değil",
                "U" => "3D Secure doğrulama yapılamadı",
                "E" => "3D Secure doğrulama hatası",
                "A" => "Kart 3D Secure için kayıtlı ancak şifre doğrulaması yapılamadı (Half Secure)",
                _ => $"Bilinmeyen durum: {status}"
            };
        }

        /// <summary>
        /// Hash hesaplama (SHA256)
        /// </summary>
        private string CalculateHash(string merchantId, string terminalNo, string orderId, string amount, string password)
        {
            var hashString = $"{merchantId}{terminalNo}{orderId}{amount}{password}";

            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(hashString);
            var hash = sha256.ComputeHash(bytes);

            return Convert.ToBase64String(hash);
        }

        /// <summary>
        /// Client IP adresini alır
        /// </summary>
        private string GetClientIp()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
                return "127.0.0.1";

            // X-Forwarded-For header'ını kontrol et (proxy/load balancer için)
            var forwardedFor = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                var ips = forwardedFor.Split(',');
                return ips[0].Trim();
            }

            // X-Real-IP header'ını kontrol et
            var realIp = httpContext.Request.Headers["X-Real-IP"].FirstOrDefault();
            if (!string.IsNullOrEmpty(realIp))
                return realIp;

            // RemoteIpAddress'i kullan
            return httpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
        }

        #endregion Private Helper Methods
    }
}