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
    public class TestEndpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("api/tests")
                .WithTags("Tests");

            // 3D Secure başlatma
            group.MapPost("/initiate", async (
                [FromServices] IPaymentService paymentService,
                CancellationToken cancellationToken) =>
            {
                var request = new PaymentInitiateRequestDto();
                request.DocumentNo = null;
                request.ClientCode = "123456";
                request.ExpiryDate = "04/28";
                request.Amount = 1000;
                request.CardNumber = "5421190122090656";
                request.CardHolderName = "Mirkan Kaçan";
                request.Cvv = "916";
                var result = await paymentService.InitiateThreeDSecureAsync(request, cancellationToken);
                return result.ToGenericResult();
            })
            .WithName("TestInitiateThreeDSecure")
            .WithSummary("Test 3D Secure ödeme işlemini başlatır");
        }
    }
}