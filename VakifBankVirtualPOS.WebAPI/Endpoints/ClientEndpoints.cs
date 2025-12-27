using Carter;
using Microsoft.AspNetCore.Mvc;
using VakifBankVirtualPOS.WebAPI.Extensions;
using VakifBankVirtualPOS.WebAPI.Services.Interfaces;

namespace VakifBankVirtualPOS.WebAPI.Endpoints
{
    /// <summary>
    /// Müşteri işlemleri için Carter endpoints
    /// </summary>
    public class ClientEndpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("api/clients")
                .WithTags("Clients");

            group.MapGet("/check/{no}", async (
                string no,
                [FromServices] IClientService clientService,
                CancellationToken cancellationToken) =>
            {
                var result = await clientService.CheckByNoAsync(no, cancellationToken);
                return result.ToGenericResult();
            })
            .WithName("CheckClientByNo")
            .WithSummary("Vergi veya TC numarasına göre cari kontrol eder varsa getirir yoksa oluşturur")
            .RequireRateLimiting("client");

            group.MapGet("/transaction/document-no/{documentNo}", async (
             string documentNo,
             [FromServices] IClientService clientService,
             CancellationToken cancellationToken) =>
            {
                var result = await clientService.GetTransactionsByDocumentAsync(documentNo, cancellationToken);
                return result.ToGenericResult();
            })
         .WithName("GetTransactionsByDocument")
         .WithSummary("Belge numarasına göre cari hareket getirir")
         .RequireRateLimiting("client");

            // Vergi numarasına göre müşteri getirme
            group.MapGet("/tax-number/{taxNumber}", async (
                string taxNumber,
                [FromServices] IClientService clientService,
                CancellationToken cancellationToken) =>
            {
                var result = await clientService.GetByTaxNumberAsync(taxNumber, cancellationToken);
                return result.ToGenericResult();
            })
            .WithName("GetClientByTaxNumber")
            .WithSummary("Vergi numarasına göre cari getirir")
            .RequireRateLimiting("client");

            // TC kimlik numarasına göre müşteri getirme
            group.MapGet("/tc-number/{tcNumber}", async (
                string tcNumber,
                [FromServices] IClientService clientService,
                CancellationToken cancellationToken) =>
            {
                var result = await clientService.GetByTcNumberAsync(tcNumber, cancellationToken);
                return result.ToGenericResult();
            })
            .WithName("GetClientByTcNumber")
            .WithSummary("TC kimlik numarasına göre cari getirir")
            .RequireRateLimiting("client");

            // Cari koduna göre müşteri getirme
            group.MapGet("/code/{clientCode}", async (
                string clientCode,
                [FromServices] IClientService clientService,
                CancellationToken cancellationToken) =>
            {
                var result = await clientService.GetByCodeAsync(clientCode, cancellationToken);
                return result.ToGenericResult();
            })
            .WithName("GetClientByCode")
            .WithSummary("Cari koduna göre cari getirir")
            .RequireRateLimiting("client");
        }
    }
}