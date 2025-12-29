using Mapster;
using System.Net;
using VakifBankPayment.WebAPI.Helpers;
using VakifBankVirtualPOS.WebAPI.Common;
using VakifBankVirtualPOS.WebAPI.Constants;
using VakifBankVirtualPOS.WebAPI.Data.Entities;
using VakifBankVirtualPOS.WebAPI.Dtos.PaymentDtos;
using VakifBankVirtualPOS.WebAPI.Helpers;
using VakifBankVirtualPOS.WebAPI.Options;
using VakifBankVirtualPOS.WebAPI.Repositories.Interfaces;
using VakifBankVirtualPOS.WebAPI.Services.Interfaces;

namespace VakifBankPayment.Services.Implementations
{
    /// <summary>
    /// VakıfBank Sanal POS ödeme servisi
    /// </summary>
    public class PaymentService : IPaymentService
    {
        private readonly VakifBankOptions _options;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<PaymentService> _logger;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IClientRepository _clientRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IEmailService _emailService;

        public PaymentService(
            VakifBankOptions options,
            IHttpClientFactory httpClientFactory,
            ILogger<PaymentService> logger,
            IPaymentRepository paymentRepository,
            IClientRepository clientRepository,
            IHttpContextAccessor httpContextAccessor,
            IEmailService emailService)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
            _clientRepository = clientRepository ?? throw new ArgumentNullException(nameof(clientRepository));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        }

        public async Task<ServiceResult<PaymentResultDto>> GetPaymentByOrderIdAsync(string orderId, CancellationToken cancellationToken)
        {
            try
            {
                var payment = await _paymentRepository.GetByOrderIdAsync(orderId, cancellationToken);

                if (payment == null)
                {
                    return ServiceResult<PaymentResultDto>.Error(
                        "Ödeme Bulunamadı",
                        $"{orderId} kodlu ödeme bulunamadı",
                        HttpStatusCode.NotFound);
                }
                var mappedPayment = payment.Adapt<PaymentResultDto>();
                mappedPayment.IsSuccess = payment.Status.ToUpper() == "SUCCESS" && payment.ResultCode == "0000" ? true : false;
                return ServiceResult<PaymentResultDto>.SuccessAsOk(payment.Adapt<PaymentResultDto>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ödeme kaydı getirilemedi. OrderId: {OrderId}", orderId);
                throw;
            }
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
                _httpContextAccessor.HttpContext?.Session.SetString("ClientCode", request.ClientCode);

                var clientIp = IpHelper.GetClientIp(_httpContextAccessor);
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

                // 5. VerifyEnrollmentRequestId oluştur (benzersiz olmalı)
                var verifyEnrollmentRequestId = Guid.NewGuid().ToString();

                // 7. Enrollment isteği hazırla
                // SessionInfo'ya OrderId gönder (VakıfBank bunu geri döndürecek)
                var formData = new Dictionary<string, string>
                {
                    { "MerchantId", _options.MerchantId },
                    { "MerchantPassword", _options.MerchantPassword },
                    { "VerifyEnrollmentRequestId", verifyEnrollmentRequestId },
                    { "Pan", cleanCardNumber },
                    { "ExpiryDate", CardHelper.ConvertExpiryDateToYYMM(request.ExpiryDate)},
                    { "PurchaseAmount", CardHelper.FormatAmount(request.Amount) },
                    { "Currency", _options.Currency },
                    { "BrandName", brandName },
                    { "SuccessUrl", _options.SuccessUrl },
                    { "FailureUrl", _options.FailureUrl },
                    { "SessionInfo", orderId } // OrderId'yi SessionInfo'da gönder
                };

                // 8. HTTP POST isteği gönder
                _logger.LogDebug("Enrollment isteği gönderiliyor. URL: {Url}", _options.EnrollmentUrl);

                var responseXml = await HttpClientHelper.PostFormDataAsync(
                    _httpClientFactory,
                    _options.EnrollmentUrl,
                    formData,
                    cancellationToken);

                // 9. XML cevabını parse et
                var enrollmentResponse = ParseEnrollmentResponse(responseXml);

                if (enrollmentResponse.Status == "Error" || enrollmentResponse.Status == "E")
                {
                    _logger.LogError("Enrollment hatası: {Message}", enrollmentResponse.Message);

                    return ServiceResult<EnrollmentResponseDto>.Error(
                        "3D Secure Hatası",
                        enrollmentResponse.Message,
                        HttpStatusCode.BadRequest);
                }

                // 10. OrderId'yi response'a ekle
                enrollmentResponse.OrderId = orderId;

                // 11. Veritabanına kaydet
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
                var clientIp = IpHelper.GetClientIp(_httpContextAccessor);
                _logger.LogInformation("3D Secure callback alındı. Status: {Status}, MdStatus: {MdStatus}, VerifyEnrollmentRequestId: {VerifyId}, IP: {ClientIp}",
                    callback.Status, callback.MdStatus, callback.VerifyEnrollmentRequestId, clientIp);

                // 1. MdStatus kontrolü (0, 1, 7)
                if (callback.MdStatus != "1")
                {
                    var mdMessage = callback.MdStatus switch
                    {
                        "0" => "3D Secure kod geçersiz veya girilmedi",
                        "7" => "3D Secure doğrulama hatası",
                        _ => $"Bilinmeyen MdStatus: {callback.MdStatus}"
                    };

                    _logger.LogWarning("3D Secure MdStatus başarısız. MdStatus: {MdStatus}", callback.MdStatus);

                    return ServiceResult<PaymentResultDto>.Error(
                        "3D Secure Başarısız",
                        mdMessage,
                        HttpStatusCode.BadRequest);
                }

                // 2. Status kontrolü (Y veya A olmalı)
                if (callback.Status != "Y" && callback.Status != "A")
                {
                    var statusMessage = GetStatusMessage(callback.Status);
                    _logger.LogWarning("3D Secure Status başarısız. Status: {Status}", callback.Status);

                    return ServiceResult<PaymentResultDto>.Error(
                        "3D Secure Doğrulama Başarısız",
                        statusMessage,
                        HttpStatusCode.BadRequest);
                }

                // 3. SessionInfo'dan OrderId'yi al
                var orderId = callback.SessionInfo;
                if (string.IsNullOrEmpty(orderId))
                {
                    _logger.LogError("SessionInfo (OrderId) bulunamadı");

                    return ServiceResult<PaymentResultDto>.Error(
                        "Geçersiz SessionInfo",
                        "SessionInfo verisinde OrderId bulunamadı",
                        HttpStatusCode.BadRequest);
                }

                _logger.LogInformation("Ödeme tamamlanıyor. OrderId: {OrderId}", orderId);

                // 5. VPOS provizyon isteği hazırla
                var vposRequest = new VposRequestDto
                {
                    MerchantId = _options.MerchantId,
                    Password = _options.MerchantPassword,
                    TransactionType = _options.TransactionType,
                    TransactionId = Guid.NewGuid().ToString().ToUpper(),
                    TerminalNo = _options.TerminalNo,
                    ClientIp = clientIp,
                    OrderId = orderId,
                    TransactionDeviceSource = _options.TransactionDeviceSource,

                    // 3D Secure için ZORUNLU alanlar
                    MpiTransactionId = callback.VerifyEnrollmentRequestId, // ZORUNLU

                    // 3D Secure için OPSİYONEL alanlar (ama önerilir)
                    ECI = callback.ECI,
                    CAVV = callback.CAVV,
                };

                // 6. XML oluştur ve VPOS'a gönder
                _logger.LogDebug("VPOS provizyon isteği gönderiliyor. URL: {Url}, Status: {Status}",
                    _options.VposUrl, callback.Status);

                var xmlRequest = XmlHelper.SerializeToXml(vposRequest);
                var xmlResponse = await HttpClientHelper.PostXmlAsync(_httpClientFactory, _options.VposUrl, xmlRequest, cancellationToken);

                // 7. Cevabı parse et
                var paymentResult = ParseVposResponse(xmlResponse);

                // 9. Sonucu kontrol et ve veritabanını güncelle
                if (!paymentResult.IsSuccess)
                {
                    await _paymentRepository.UpdateByOrderIdAsync(
                        orderId: orderId,
                        status: "Failed",
                        cancellationToken: cancellationToken,
                        transactionId: paymentResult.TransactionId,
                        resultCode: paymentResult.ResultCode,
                        errorMessage: paymentResult.Message
                    );
                    await _emailService.SendPaymentFailedMailAsync(orderId, cancellationToken);
                    _logger.LogError("Ödeme başarısız. OrderId: {OrderId}, ErrorCode: {ErrorCode}, Message: {Message}",
                        orderId, paymentResult.ResultCode, paymentResult.Message);

                    return ServiceResult<PaymentResultDto>.Error(
                        "Ödeme Başarısız",
                        paymentResult.Message,
                        HttpStatusCode.BadRequest);
                }

                await _paymentRepository.UpdateByOrderIdAsync(
                    orderId: orderId,
                    status: "Success",
                    cancellationToken: cancellationToken,
                    resultCode: paymentResult.ResultCode,
                    transactionId: paymentResult.TransactionId,
                    authCode: paymentResult.AuthCode
                );

                await _clientRepository.CreateTransactionAsync(orderId, cancellationToken);
                await _emailService.SendPaymentSuccessMailAsync(orderId, cancellationToken);
                _logger.LogInformation("Ödeme başarılı. OrderId: {OrderId}, TransactionId: {TransactionId}, AuthCode: {AuthCode}, 3DStatus: {Status}",
                    orderId, paymentResult.TransactionId, paymentResult.AuthCode, callback.Status);

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
                    Status = XmlHelper.GetElementValue(xmlResponse, "Status"),
                    Message = XmlHelper.GetElementValue(xmlResponse, "Message"),
                    MessageErrorCode = XmlHelper.GetElementValue(xmlResponse, "MessageErrorCode")
                };

                // Status kontrolü
                if (result.Status == "Y")
                {
                    // Başarılı - 3D Secure'a devam et
                    result.ACSUrl = XmlHelper.GetElementValue(xmlResponse, "ACSUrl");
                    result.PAReq = XmlHelper.GetElementValue(xmlResponse, "PAReq");
                    result.TermUrl = XmlHelper.GetElementValue(xmlResponse, "TermUrl");
                    result.MD = XmlHelper.GetElementValue(xmlResponse, "MD");
                }
                else if (result.Status == "E")
                {
                    // Hata
                    var errorCode = XmlHelper.GetElementValue(xmlResponse, "ErrorCode");
                    var errorMessage = XmlHelper.GetElementValue(xmlResponse, "ErrorMessage");
                }
                result.Message = GetStatusMessage(result.Status);

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
                var resultCode = XmlHelper.GetElementValue(xmlResponse, "ResultCode");
                var resultDetail = XmlHelper.GetElementValue(xmlResponse, "ResultDetail");
                var transactionId = XmlHelper.GetElementValue(xmlResponse, "TransactionId");
                var authCode = XmlHelper.GetElementValue(xmlResponse, "AuthCode");
                var orderId = XmlHelper.GetElementValue(xmlResponse, "OrderId");

                var isSuccess = VakifBankResponseCodes.IsSuccess(resultCode);
                var codeMessage = VakifBankResponseCodes.GetMessage(resultCode);
                return new PaymentResultDto
                {
                    IsSuccess = isSuccess,
                    Message = isSuccess ? codeMessage : $"{resultDetail} - {codeMessage}",
                    TransactionId = transactionId,
                    AuthCode = authCode,
                    OrderId = orderId,
                    ResultCode = resultCode
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "VPOS XML parse hatası");
                throw;
            }
        }

        /// <summary>
        /// Status mesajını döndürür
        /// </summary>
        private string GetStatusMessage(string status)
        {
            return status switch
            {
                "Y" => "3D Secure doğrulama başarılı (Full Secure)",
                "A" => "3D Secure doğrulama kısmen başarılı (Half Secure)",
                "N" => "Kart 3D Secure programına kayıtlı değil veya işlem reddedildi",
                "U" => "3D Secure doğrulama tamamlanamadı",
                "E" => "3D Secure doğrulama hatası",
                _ => $"Bilinmeyen durum: {status}"
            };
        }

        #endregion Private Helper Methods
    }
}