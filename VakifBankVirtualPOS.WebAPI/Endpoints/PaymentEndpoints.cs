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
                [FromServices] IVakifBankService paymentService,
                CancellationToken cancellationToken) =>
            {
                var result = await paymentService.InitiateThreeDSecureAsync(request, cancellationToken);
                return result.ToGenericResult();
            })
            .WithName("InitiateThreeDSecure")
            .WithSummary("3D Secure ödeme işlemini başlatır")
            .RequireRateLimiting("payment");

            // 3D Secure callback
            group.MapPost("/3d-callback", async (
                [FromForm] ThreeDCallbackDto callback,
                [FromServices] IVakifBankService paymentService,
                CancellationToken cancellationToken) =>
            {
                var result = await paymentService.CompletePaymentAsync(callback, cancellationToken);
                return result.ToGenericResult();
            })
            .WithName("ThreeDCallback")
            .WithSummary("3D Secure doğrulama sonrası callback")
            .DisableAntiforgery()
            .RequireRateLimiting("payment");
        }
    }
}