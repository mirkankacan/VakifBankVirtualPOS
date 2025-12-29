using Microsoft.AspNetCore.Mvc;
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
                return BadRequest("Vergi numarası boş gönderilemez");

            var client = await _clientApiService.GetByTaxNumberAsync(taxNumber, cancellationToken);

            return client.IsSuccess ? Ok(client.Data) : BadRequest(client.ErrorMessage);
        }

        [HttpGet("tc-no/{tcNumber}")]
        public async Task<IActionResult> GetByTcNumber(string tcNumber, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(tcNumber))
                return BadRequest("TC Kimlik numarası boş gönderilemez");

            var client = await _clientApiService.GetByTcNumberAsync(tcNumber, cancellationToken);
            return client.IsSuccess ? Ok(client.Data) : BadRequest(client.ErrorMessage);
        }

        [HttpGet("no/{no}")]
        public async Task<IActionResult> CheckByNo(string no, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(no))
                return BadRequest("HYBS'den gelen vergi numarası ya da TC kimlik numarası boş");

            var client = await _clientApiService.CheckByNoAsync(no, cancellationToken);
            return client.IsSuccess ? Ok(client.Data) : BadRequest(client.ErrorMessage);
        }
    }
}