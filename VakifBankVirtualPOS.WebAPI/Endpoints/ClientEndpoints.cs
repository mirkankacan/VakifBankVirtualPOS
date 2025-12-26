using Carter;
using Microsoft.AspNetCore.Mvc;
using VakifBankVirtualPOS.WebAPI.Dtos;
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

            // Müşteri oluşturma
            group.MapPost("/", async (
                [FromBody] CreateClientRequestDto request,
                [FromServices] IClientService clientService,
                CancellationToken cancellationToken) =>
            {
                var result = await clientService.CreateClientAsync(request, cancellationToken);
                return result.ToGenericResult();
            })
            .WithName("CreateClient")
            .WithSummary("Yeni müşteri oluşturur");

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
            .WithSummary("Vergi numarasına göre müşteri getirir");

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
            .WithSummary("TC kimlik numarasına göre müşteri getirir");

            // Health check
            group.MapGet("/health", () =>
            {
                return Results.Ok(new
                {
                    status = "healthy",
                    service = "ClientService",
                    timestamp = DateTime.Now
                });
            })
            .WithName("ClientHealthCheck")
            .WithSummary("Müşteri servisi sağlık kontrolü");
        }
    }
}
