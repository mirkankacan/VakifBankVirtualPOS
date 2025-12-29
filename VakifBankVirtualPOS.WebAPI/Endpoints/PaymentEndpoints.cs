using Carter;
using Microsoft.AspNetCore.Mvc;
using VakifBankVirtualPOS.WebAPI.Dtos.PaymentDtos;
using VakifBankVirtualPOS.WebAPI.Extensions;
using VakifBankVirtualPOS.WebAPI.Services.Interfaces;

namespace VakifBankPayment.WebAPI.Endpoints
{
    /// <summary>
    /// Ödeme işlemleri için Carter endpoints
    /// </summary>
    public class PaymentEndpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("api/payments")
                .WithTags("Payments");

            // 3D Secure başlatma
            group.MapPost("/initiate", async (
                [FromBody] PaymentInitiateRequestDto request,
                [FromServices] IPaymentService paymentService,
                CancellationToken cancellationToken) =>
            {
                var result = await paymentService.InitiateThreeDSecureAsync(request, cancellationToken);
                return result.ToGenericResult();
            })
            .WithName("InitiateThreeDSecure")
            .WithSummary("3D Secure ödeme işlemini başlatır")
            .RequireRateLimiting("payment");
            group.MapGet("/{orderId}", async (
            string orderId,
             [FromServices] IPaymentService paymentService,
             CancellationToken cancellationToken) =>
            {

                var result = await paymentService.GetPaymentByOrderIdAsync(orderId, cancellationToken);
                return result.ToGenericResult();
            })
         .WithName("GetPaymentByOrderId")
         .WithSummary("Ödeme no'suna göre bir ödemeyi getirir")
         .RequireRateLimiting("payment");

            // 3D Secure callback
            group.MapPost("/3d-callback", async ([FromForm] ThreeDCallbackDto callback,
                [FromServices] IPaymentService paymentService, HttpContext context,
                CancellationToken cancellationToken) =>
            {
                var result = await paymentService.CompletePaymentAsync(callback, cancellationToken);
                var redirectUrl = result.IsSuccess
                     ? $"https://localhost:8484/odeme/basarili/{result.Data.OrderId}"
                     : $"https://localhost:8484/odeme/basarisiz/{result.Data.OrderId}";

                var html = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <meta http-equiv=""refresh"" content=""0;url={redirectUrl}"">
    <title>Yönlendiriliyor...</title>
</head>
<body>
    <div style=""text-align: center; padding: 50px; font-family: Arial, sans-serif;"">
        <h2>Ödeme işlemi tamamlandı</h2>
        <p>Yönlendiriliyor...</p>
        <script>
            window.location.href = '{redirectUrl}';
        </script>
    </div>
</body>
</html>";

                context.Response.ContentType = "text/html; charset=utf-8";
                await context.Response.WriteAsync(html, cancellationToken);

                return Results.Empty;
            })
            .WithName("ThreeDCallback")
            .WithSummary("3D Secure doğrulama sonrası callback")
            .DisableAntiforgery()
            .AllowAnonymous();
        }
    }
}