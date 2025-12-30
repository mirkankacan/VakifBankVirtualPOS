using System.Net;
using Microsoft.AspNetCore.Mvc;
using VakifBankVirtualPOS.WebUI.Extensions;
using VakifBankVirtualPOS.WebUI.Services.Interfaces;

namespace VakifBankVirtualPOS.WebUI.Controllers
{
    [Route("cari")]
    public class ClientController : Controller
    {
        private readonly IClientApiService _clientApiService;

        public ClientController(IClientApiService clientApiService)
        {
            _clientApiService = clientApiService;
        }

        [HttpGet("vergi-no/{taxNumber}")]
        public async Task<IActionResult> GetByTaxNumber(string taxNumber, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(taxNumber))
            {
                return Problem(
                    title: "Doğrulama Hatası",
                    detail: "Vergi numarası boş gönderilemez",
                    statusCode: (int)HttpStatusCode.BadRequest
                );
            }

            var client = await _clientApiService.GetByTaxNumberAsync(taxNumber, cancellationToken);
            return client.ToActionResult();
        }

        [HttpGet("tc-no/{tcNumber}")]
        public async Task<IActionResult> GetByTcNumber(string tcNumber, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(tcNumber))
            {
                return Problem(
                    title: "Doğrulama Hatası",
                    detail: "TC Kimlik numarası boş gönderilemez",
                    statusCode: (int)HttpStatusCode.BadRequest
                );
            }

            var client = await _clientApiService.GetByTcNumberAsync(tcNumber, cancellationToken);
            return client.ToActionResult();
        }

        [HttpGet("no/{no}")]
        public async Task<IActionResult> CheckByNo(string no, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(no))
            {
                return Problem(
                    title: "Doğrulama Hatası",
                    detail: "HYBS'den gelen vergi numarası ya da TC kimlik numarası boş",
                    statusCode: (int)HttpStatusCode.BadRequest
                );
            }

            var client = await _clientApiService.CheckByNoAsync(no, cancellationToken);
            return client.ToActionResult();
        }
    }
}