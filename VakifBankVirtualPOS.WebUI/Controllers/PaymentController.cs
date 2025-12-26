using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using VakifBankVirtualPOS.WebUI.Models;

namespace VakifBankVirtualPOS.WebUI.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<PaymentController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Test ödeme sayfası
        /// </summary>
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Test endpoint'ine istek atar ve sonucu gösterir
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Initiate()
        {
            try
            {
                var apiBaseUrl = _configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5272";
                var httpClient = _httpClientFactory.CreateClient();

                _logger.LogInformation("Test endpoint'ine istek atılıyor...");

                // Test endpoint'ine POST isteği (body olmadan - endpoint hardcoded değerler kullanıyor)
                var response = await httpClient.PostAsync($"{apiBaseUrl}/api/tests/initiate", null);

                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Test endpoint hatası. Status: {Status}, Error: {Error}",
                        response.StatusCode, responseContent);

                    TempData["Error"] = $"Hata: {response.StatusCode} - {responseContent}";
                    TempData["Response"] = responseContent;
                    return RedirectToAction("TestResult");
                }

                _logger.LogInformation("Test endpoint başarılı. Response: {Response}", responseContent);

                // Response'u parse et
                var enrollmentResponse = JsonSerializer.Deserialize<EnrollmentResponse>(responseContent,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (enrollmentResponse == null)
                {
                    TempData["Error"] = "Response parse edilemedi";
                    TempData["Response"] = responseContent;
                    return RedirectToAction("TestResult");
                }

                // Status ne olursa olsun 3D Secure sayfasına yönlendir
                _logger.LogInformation("3D Secure başlatılıyor. OrderId: {OrderId}, Status: {Status}",
                    enrollmentResponse.OrderId, enrollmentResponse.Status);

                // Eğer değerler null ise message'dan parse etmeyi dene
                var acsUrl = enrollmentResponse.ACSUrl;
                var termUrl = enrollmentResponse.TermUrl;

                if (string.IsNullOrEmpty(acsUrl) && !string.IsNullOrEmpty(enrollmentResponse.Message))
                {
                    // Message içinden URL'leri bul
                    var message = enrollmentResponse.Message;
                    var urlPattern = @"https?://[^\s]+";
                    var urls = System.Text.RegularExpressions.Regex.Matches(message, urlPattern);

                    if (urls.Count > 0)
                    {
                        acsUrl = urls[0].Value;
                        _logger.LogInformation("ACS URL message'dan parse edildi: {Url}", acsUrl);
                    }

                    if (urls.Count > 1)
                    {
                        termUrl = urls[1].Value;
                        _logger.LogInformation("Term URL message'dan parse edildi: {Url}", termUrl);
                    }
                }

                TempData["ACSUrl"] = acsUrl;
                TempData["PAReq"] = enrollmentResponse.PAReq;
                TempData["TermUrl"] = termUrl ?? _configuration["VakifBankOptions:SuccessUrl"] ?? $"{Request.Scheme}://{Request.Host}/Payment/Callback";
                TempData["MD"] = enrollmentResponse.MD;
                TempData["OrderId"] = enrollmentResponse.OrderId;
                TempData["Status"] = enrollmentResponse.Status;
                TempData["Message"] = enrollmentResponse.Message;

                return RedirectToAction("ThreeDSecure");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Test endpoint isteği hatası");
                TempData["Error"] = $"Bir hata oluştu: {ex.Message}";
                TempData["Response"] = ex.ToString();
                return RedirectToAction("TestResult");
            }
        }

        /// <summary>
        /// 3D Secure yönlendirme sayfası
        /// </summary>
        [HttpGet]
        public IActionResult ThreeDSecure()
        {
            ViewBag.ACSUrl = TempData["ACSUrl"];
            ViewBag.PAReq = TempData["PAReq"];
            ViewBag.TermUrl = TempData["TermUrl"];
            ViewBag.MD = TempData["MD"];
            ViewBag.OrderId = TempData["OrderId"];

            return View();
        }

        /// <summary>
        /// Status mesajını döndürür
        /// </summary>
        private string GetStatusMessage(string status)
        {
            return status switch
            {
                "Y" => "Kart 3D Secure programına kayıtlı - Doğrulama yapılabilir",
                "N" => "Kart 3D Secure programına kayıtlı değil",
                "U" => "3D Secure doğrulama yapılamadı",
                "E" => "3D Secure doğrulama hatası",
                "A" => "Kart 3D Secure için kayıtlı ancak şifre doğrulaması yapılamadı (Half Secure)",
                _ => $"Bilinmeyen durum: {status}"
            };
        }

        /// <summary>
        /// Test sonuç sayfası
        /// </summary>
        [HttpGet]
        public IActionResult TestResult()
        {
            ViewBag.Success = TempData["Success"];
            ViewBag.Error = TempData["Error"];
            ViewBag.Response = TempData["Response"];
            return View();
        }

        /// <summary>
        /// 3D Secure callback sayfası (bankadan dönen sonuçları işler)
        /// </summary>
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Callback(ThreeDCallbackViewModel model)
        {
            try
            {
                _logger.LogInformation("3D Secure callback alındı. Status: {Status}", model.Status);

                var apiBaseUrl = _configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5272";
                var httpClient = _httpClientFactory.CreateClient();

                var callbackDto = new
                {
                    MD = model.MD,
                    Status = model.Status,
                    Eci = model.Eci,
                    Cavv = model.Cavv,
                    VerifyEnrollmentRequestId = model.VerifyEnrollmentRequestId,
                    MpiTransactionId = model.MpiTransactionId
                };

                var formData = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("MD", model.MD ?? ""),
                    new KeyValuePair<string, string>("PaRes", model.PaRes ?? ""),
                    new KeyValuePair<string, string>("Status", model.Status ?? ""),
                    new KeyValuePair<string, string>("Eci", model.Eci ?? ""),
                    new KeyValuePair<string, string>("Cavv", model.Cavv ?? ""),
                    new KeyValuePair<string, string>("VerifyEnrollmentRequestId", model.VerifyEnrollmentRequestId ?? ""),
                    new KeyValuePair<string, string>("MpiTransactionId", model.MpiTransactionId ?? "")
                });

                var response = await httpClient.PostAsync($"{apiBaseUrl}/api/payments/3d-callback", formData);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Ödeme tamamlama hatası. Status: {Status}, Error: {Error}",
                        response.StatusCode, errorContent);

                    return RedirectToAction("Result", new { isSuccess = false, message = "Ödeme tamamlanamadı." });
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var paymentResult = JsonSerializer.Deserialize<PaymentResultDto>(responseContent,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (paymentResult == null)
                {
                    return RedirectToAction("Result", new { isSuccess = false, message = "Ödeme sonucu alınamadı." });
                }

                return RedirectToAction("Result", new
                {
                    isSuccess = paymentResult.IsSuccess,
                    message = paymentResult.Message,
                    transactionId = paymentResult.TransactionId,
                    authCode = paymentResult.AuthCode,
                    orderId = paymentResult.OrderId,
                    errorCode = paymentResult.ErrorCode
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Callback işleme hatası");
                return RedirectToAction("Result", new { isSuccess = false, message = "Bir hata oluştu." });
            }
        }

        /// <summary>
        /// Ödeme sonuç sayfası
        /// </summary>
        [HttpGet]
        public IActionResult Result(bool isSuccess, string message, string? transactionId = null,
            string? authCode = null, string? orderId = null, string? errorCode = null)
        {
            var viewModel = new PaymentResultViewModel
            {
                IsSuccess = isSuccess,
                Message = message ?? "Bilinmeyen hata",
                TransactionId = transactionId,
                AuthCode = authCode,
                OrderId = orderId,
                ErrorCode = errorCode
            };

            return View(viewModel);
        }

        /// <summary>
        /// MM/YY formatını YYMM formatına çevirir
        /// </summary>
        private string ConvertExpiryDateToYYMM(string expiryDate)
        {
            if (string.IsNullOrWhiteSpace(expiryDate))
                return string.Empty;

            // MM/YY formatından MM ve YY'yi al
            var parts = expiryDate.Split('/');
            if (parts.Length != 2)
                return expiryDate;

            var month = parts[0].PadLeft(2, '0');
            var year = parts[1].PadLeft(2, '0');

            // YYMM formatına çevir
            return $"{year}{month}";
        }

        // DTO classes for deserialization
        private class EnrollmentResponse
        {
            public string OrderId { get; set; } = string.Empty;
            public string Status { get; set; } = string.Empty;
            public string Message { get; set; } = string.Empty;
            public string MessageErrorCode { get; set; } = string.Empty;
            public string ACSUrl { get; set; } = string.Empty;
            public string PAReq { get; set; } = string.Empty;
            public string TermUrl { get; set; } = string.Empty;
            public string MD { get; set; } = string.Empty;
        }

        private class PaymentResultDto
        {
            public bool IsSuccess { get; set; }
            public string Message { get; set; } = string.Empty;
            public string? TransactionId { get; set; }
            public string? AuthCode { get; set; }
            public string? OrderId { get; set; }
            public string? ErrorCode { get; set; }
        }
    }
}