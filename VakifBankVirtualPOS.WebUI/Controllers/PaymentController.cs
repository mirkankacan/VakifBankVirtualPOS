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
        /// Ödeme başlatma sayfası
        /// </summary>
        [HttpGet]
        public IActionResult Index()
        {
            return View(new PaymentInitiateViewModel());
        }

        /// <summary>
        /// Test endpoint'ine istek atar
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Initiate(PaymentInitiateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }

            try
            {
                var apiBaseUrl = _configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5272";
                var httpClient = _httpClientFactory.CreateClient();

                _logger.LogInformation("Test endpoint'ine istek atılıyor. Tutar: {Amount}, Cari Kod: {ClientCode}",
                    model.Amount, model.ClientCode);

                // Test endpoint'ine POST isteği (body olmadan - endpoint hardcoded değerler kullanıyor)
                var response = await httpClient.PostAsync($"{apiBaseUrl}/api/tests/initiate", null);

                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Test endpoint hatası. Status: {Status}, Error: {Error}",
                        response.StatusCode, responseContent);

                    ViewBag.Error = $"Hata: {response.StatusCode} - {responseContent}";
                    ViewBag.Response = responseContent;
                    return View("TestResult");
                }

                _logger.LogInformation("Test endpoint başarılı. Response: {Response}", responseContent);

                ViewBag.Success = true;
                ViewBag.Response = responseContent;
                return View("TestResult");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Test endpoint isteği hatası");
                ViewBag.Error = $"Bir hata oluştu: {ex.Message}";
                ViewBag.Response = ex.ToString();
                return View("TestResult");
            }
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