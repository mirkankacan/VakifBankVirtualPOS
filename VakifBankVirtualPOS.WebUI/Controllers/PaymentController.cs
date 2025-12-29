using Microsoft.AspNetCore.Mvc;
using VakifBankVirtualPOS.WebUI.Models;

namespace VakifBankVirtualPOS.WebUI.Controllers
{
    [Route("odeme")]
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
        /// Ödeme işlemi sayfası
        /// </summary>
        [HttpGet("")]
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Ödeme işlemini başlatır ve 3D Secure sürecini başlatır
        /// </summary>
        [HttpPost("baslat")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Initiate()
        {
            try
            {
                return RedirectToAction("ThreeDSecure");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ödeme başlatma hatası");

                return StatusCode(500, ex);
            }
        }

        /// <summary>
        /// 3D Secure yönlendirme sayfası
        /// </summary>
        [HttpGet("3d-dogrulama")]
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
        /// 3D Secure callback sayfası (bankadan dönen sonuçları işler)
        /// </summary>
        [HttpPost("3d-bildirim")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Callback(ThreeDCallbackViewModel model)
        {
            try
            {
                return RedirectToAction("Success");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Callback işleme hatası");
                return StatusCode(500, ex);
            }
        }

        [HttpGet("basarili/{orderId}")]
        public IActionResult Success(string orderId, CancellationToken cancellationToken)
        {
            return View();
        }

        [HttpGet("basarisiz/{orderId?}")]
        public IActionResult Failed(string? orderId, CancellationToken cancellationToken)
        {
            return View();
        }
    }
}