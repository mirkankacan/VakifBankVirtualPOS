using Carter;
using Microsoft.AspNetCore.Mvc;
using VakifBankVirtualPOS.WebAPI.Dtos.PaymentDtos;
using VakifBankVirtualPOS.WebAPI.Extensions;
using VakifBankVirtualPOS.WebAPI.Options;
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
                [FromServices] IPaymentService paymentService, HttpContext context, UiOptions options,
                CancellationToken cancellationToken) =>
            {
                try
                {
                    var result = await paymentService.CompletePaymentAsync(callback, cancellationToken);
                    var orderId = result.Data?.OrderId ?? callback.SessionInfo;

                    // Success durumunda orderId zorunludur
                    var redirectUrl = result.IsSuccess ? $"{options.PaymentSuccessUrl}/{orderId}" : !string.IsNullOrEmpty(orderId) ? $"{options.PaymentFailureUrl}/{orderId}" : options.PaymentFailureUrl;

                    var logoUrl = $"{options.BaseUrl}/theme/assets/images/egesehir.png";
                    var isSuccess = result.IsSuccess;

                    var html = $@"
<!DOCTYPE html>
<html lang=""tr"">
<head>
    <meta charset=""utf-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
        <meta name=""robots"" content=""noindex, nofollow, noarchive, nosnippet"">
    <meta http-equiv=""refresh"" content=""0;url={redirectUrl}"">
    <title>Yönlendiriliyor...</title>
    <link href=""https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css"" rel=""stylesheet"">
    <link rel=""stylesheet"" href=""https://cdnjs.cloudflare.com/ajax/libs/font-awesome/7.0.0/css/all.min.css"">
    <style>
        body {{
            background: linear-gradient(135deg, #f5f7fa 0%, #c3cfe2 100%);
            min-height: 100vh;
            display: flex;
            align-items: center;
            justify-content: center;
            font-family: 'Poppins', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
        }}
        .card {{
            border: none;
            border-radius: 15px;
            box-shadow: 0 10px 30px rgba(0,0,0,0.1);
            max-width: 500px;
            width: 100%;
        }}
        .card-body {{
            padding: 2rem;
        }}
        .btn-egesehir {{
            background-color: #9c182f;
            border-color: #9c182f;
            color: #ffffff;
            padding: 0.75rem 2rem;
            border-radius: 5px;
            font-weight: 500;
        }}
        .btn-egesehir:hover {{
            background-color: #7d1225;
            border-color: #7d1225;
            color: #ffffff;
            box-shadow: 0 7px 23px -8px rgba(156, 24, 47, 0.5);
        }}
        .spinner-border {{
            width: 3rem;
            height: 3rem;
        }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""row justify-content-center"">
            <div class=""col-md-6"">
                <div class=""card"">
                    <div class=""card-body"">
                        <h5 class=""card-title text-center mb-4"">
                            <img src=""{logoUrl}"" class=""img-fluid"" alt=""Egeşehir"" style=""max-height: 80px;"">
                        </h5>
                        <div class=""text-center"">
                            <div class=""mb-4"">
                                <div class=""spinner-border text-primary"" role=""status"">
                                    <span class=""visually-hidden"">Yükleniyor...</span>
                                </div>
                            </div>
                            <h3 class=""mb-3"">Ödeme İşlemi Tamamlanıyor</h3>
                            <p class=""text-muted mb-4"">Yönlendiriliyorsunuz, lütfen bekleyiniz...</p>
                            {(isSuccess ? $@"<div class=""alert alert-info mb-3"">
                                <strong>Sipariş No:</strong> {orderId}
                            </div>" : "")}
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
    <script>
        window.location.href = '{redirectUrl}';
    </script>
</body>
</html>";

                    context.Response.ContentType = "text/html; charset=utf-8";
                    await context.Response.WriteAsync(html, cancellationToken);

                    return Results.Empty;
                }
                catch (Exception)
                {
                    var errorUrl = $"{options.PaymentFailureUrl}";
                    var logoUrl = $"{options.BaseUrl}/theme/assets/images/egesehir.png";

                    var errorHtml = $@"
<!DOCTYPE html>
<html lang=""tr"">
<head>
    <meta charset=""utf-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
        <meta name=""robots"" content=""noindex, nofollow, noarchive, nosnippet"">
    <meta http-equiv=""refresh"" content=""3;url={errorUrl}"">
    <title>Hata Oluştu</title>
    <link href=""https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css"" rel=""stylesheet"">
    <link rel=""stylesheet"" href=""https://cdnjs.cloudflare.com/ajax/libs/font-awesome/7.0.0/css/all.min.css"">
    <style>
        body {{
            background: linear-gradient(135deg, #f5f7fa 0%, #c3cfe2 100%);
            min-height: 100vh;
            display: flex;
            align-items: center;
            justify-content: center;
            font-family: 'Poppins', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
        }}
        .card {{
            border: none;
            border-radius: 15px;
            box-shadow: 0 10px 30px rgba(0,0,0,0.1);
            max-width: 500px;
            width: 100%;
        }}
        .card-body {{
            padding: 2rem;
        }}
        .btn-egesehir {{
            background-color: #9c182f;
            border-color: #9c182f;
            color: #ffffff;
            padding: 0.75rem 2rem;
            border-radius: 5px;
            font-weight: 500;
        }}
        .btn-egesehir:hover {{
            background-color: #7d1225;
            border-color: #7d1225;
            color: #ffffff;
            box-shadow: 0 7px 23px -8px rgba(156, 24, 47, 0.5);
        }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""row justify-content-center"">
            <div class=""col-md-6"">
                <div class=""card"">
                    <div class=""card-body"">
                        <h5 class=""card-title text-center mb-4"">
                            <img src=""{logoUrl}"" class=""img-fluid"" alt=""Egeşehir"" style=""max-height: 80px;"">
                        </h5>
                        <div class=""text-center"">
                            <div class=""mb-4"">
                                <i class=""fa-solid fa-circle-xmark text-danger"" style=""font-size: 5rem;""></i>
                            </div>
                            <h3 class=""text-danger mb-3"">Bir Hata Oluştu</h3>
                            <p class=""text-muted mb-4"">Ödeme işlemi tamamlanamadı. Yönlendiriliyorsunuz...</p>
                            <div class=""alert alert-danger"">
                                <strong>Hata:</strong> Ödeme işlemi sırasında bir sorun oluştu.
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
    <script>
        setTimeout(function() {{
            window.location.href = '{errorUrl}';
        }}, 3000);
    </script>
</body>
</html>";

                    context.Response.ContentType = "text/html; charset=utf-8";
                    context.Response.StatusCode = 200;
                    await context.Response.WriteAsync(errorHtml, cancellationToken);

                    return Results.Empty;
                }
            })
            .WithName("ThreeDCallback")
            .WithSummary("3D Secure doğrulama sonrası callback")
            .DisableAntiforgery()
            .AllowAnonymous();
        }
    }
}