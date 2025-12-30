using Microsoft.AspNetCore.Mvc;
using VakifBankVirtualPOS.WebUI.Extensions;
using VakifBankVirtualPOS.WebUI.Models.PaymentViewModels;
using VakifBankVirtualPOS.WebUI.Services.Interfaces;

namespace VakifBankVirtualPOS.WebUI.Controllers
{
    [Route("odeme")]
    public class PaymentController : Controller
    {
        private readonly IPaymentApiService _paymentApiService;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(
            IPaymentApiService paymentApiService,
            ILogger<PaymentController> logger)
        {
            _paymentApiService = paymentApiService;
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
        public async Task<IActionResult> Initiate([FromBody] PaymentInitiateViewModel model)
        {
            try
            {
                var result = await _paymentApiService.InitiateThreeDSecureAsync(model);

                if (!result.IsSuccess)
                {
                    return result.ToActionResult();
                }

                HttpContext.Session.SetString("ACSUrl", result.Data.ACSUrl);
                HttpContext.Session.SetString("PAReq", result.Data.PAReq);
                HttpContext.Session.SetString("TermUrl", result.Data.TermUrl);
                HttpContext.Session.SetString("MD", result.Data.MD);
                HttpContext.Session.SetString("OrderId", result.Data.OrderId);
                HttpContext.Session.SetString("Status", result.Data.Status);
                HttpContext.Session.SetString("Message", result.Data.Message);

                return Ok(new { redirectUrl = Url.Action("ThreeDSecure", "Payment") });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ödeme başlatma hatası");
                return RedirectToAction(nameof(Failed));
            }
        }

        /// <summary>
        /// 3D Secure yönlendirme sayfası
        /// </summary>
        [HttpGet("3d-dogrulama")]
        public IActionResult ThreeDSecure()
        {
            ViewBag.ACSUrl = HttpContext.Session.GetString("ACSUrl");
            ViewBag.PAReq = HttpContext.Session.GetString("PAReq");
            ViewBag.TermUrl = HttpContext.Session.GetString("TermUrl");
            ViewBag.MD = HttpContext.Session.GetString("MD");
            ViewBag.OrderId = HttpContext.Session.GetString("OrderId");
            ClearPaymentSession();

            if (string.IsNullOrEmpty(ViewBag.ACSUrl))
            {
                return RedirectToAction(nameof(Failed));
            }

            return View();
        }

        [HttpGet("basarili/{orderId}")]
        public async Task<IActionResult> Success(string orderId, CancellationToken cancellationToken)
        {
            var payment = await _paymentApiService.GetPaymentByOrderIdAsync(orderId, cancellationToken);
            if (payment.IsSuccess)
            {
                if (payment.Data.ResultCode != "0000")
                {
                    return Redirect($"/odeme/basarisiz/{orderId}");
                }
                return View(payment.Data);
            }
            return View();

        }

        [HttpGet("basarisiz/{orderId?}")]
        public async Task<IActionResult> Failed(string? orderId, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrEmpty(orderId))
            {
                var payment = await _paymentApiService.GetPaymentByOrderIdAsync(orderId, cancellationToken);
                if (payment.IsSuccess)
                {

                    if (payment.Data.ResultCode == "0000")
                    {
                        return Redirect($"/odeme/basarili/{orderId}");
                    }
                    return View(payment.Data);
                }
            }
            return View();
        }


        private void ClearPaymentSession()
        {
            HttpContext.Session.Remove("ACSUrl");
            HttpContext.Session.Remove("PAReq");
            HttpContext.Session.Remove("TermUrl");
            HttpContext.Session.Remove("MD");
            HttpContext.Session.Remove("OrderId");
            HttpContext.Session.Remove("Status");
            HttpContext.Session.Remove("Message");
        }
    }
}