using Mapster;
using System.Net;
using VakifBankVirtualPOS.WebAPI.Common;
using VakifBankVirtualPOS.WebAPI.Dtos.ClientDtos;
using VakifBankVirtualPOS.WebAPI.Dtos.HybsDtos;
using VakifBankVirtualPOS.WebAPI.Repositories.Interfaces;
using VakifBankVirtualPOS.WebAPI.Services.Interfaces;

namespace VakifBankVirtualPOS.WebAPI.Services.Implementations
{
    public class ClientService : IClientService
    {
        private readonly IClientRepository _clientRepository;
        private readonly ILogger<IClientService> _logger;
        private readonly IHybsService _hybsService;

        public ClientService(
            IClientRepository clientRepository,
            ILogger<IClientService> logger,
            IHybsService hybsService)
        {
            _clientRepository = clientRepository ?? throw new ArgumentNullException(nameof(clientRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _hybsService = hybsService ?? throw new ArgumentNullException(nameof(hybsService));
        }

        public async Task<ServiceResult<ClientDto>> CheckByNoAsync(string no, CancellationToken cancellationToken)
        {
            try
            {
                var client = await _clientRepository.CheckByNoAsync(no, cancellationToken);

                if (client != null)
                {
                    return ServiceResult<ClientDto>.SuccessAsOk(client.Adapt<ClientDto>());
                }

                var clientDetails = await _hybsService.GetClientDetailByNoAsync(no, cancellationToken);
                if (clientDetails == null)
                {
                    return ServiceResult<ClientDto>.Error("HYBS sisteminde cari bilgileri bulunamadı", HttpStatusCode.NotFound);
                }
                var newClient = await _clientRepository.CreateClientAsync(clientDetails, cancellationToken);
                if (newClient == null)
                {
                    return ServiceResult<ClientDto>.Error(
                        "Cari Oluşturma Hatası",
                        "Cari kaydı oluşturulamadı. Aynı cari ismiyle kayıt mevcut olabilir.",
                        HttpStatusCode.InternalServerError);
                }
                return ServiceResult<ClientDto>.SuccessAsOk(newClient.Adapt<ClientDto>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Numara ile cari kontrol işleminde hata. No: {No}", no);
                throw;
            }
        }

        public async Task<ServiceResult<ClientDto>> GetByCodeAsync(string clientCode, CancellationToken cancellationToken)
        {
            try
            {
                var client = await _clientRepository.GetByCodeAsync(clientCode, cancellationToken);

                if (client == null)
                {
                    return ServiceResult<ClientDto>.Error(
                        "Cari bulunamadı",
                        $"{clientCode} kodlu cari bulunamadı",
                        HttpStatusCode.NotFound);
                }

                return ServiceResult<ClientDto>.SuccessAsOk(client.Adapt<ClientDto>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cari koda göre cari getirme işleminde hata. ClientCode: {ClientCode}", clientCode);
                throw;
            }
        }

        public async Task<ServiceResult<ClientDto>> GetByTaxNumberAsync(string taxNumber, CancellationToken cancellationToken)
        {
            try
            {
                var client = await _clientRepository.GetByTaxNumberAsync(taxNumber, cancellationToken);

                if (client == null)
                {
                    return ServiceResult<ClientDto>.Error(
                        "Cari Bulunamadı",
                        $"{taxNumber} vergi numaralı cari bulunamadı",
                        HttpStatusCode.NotFound);
                }

                return ServiceResult<ClientDto>.SuccessAsOk(client.Adapt<ClientDto>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Vergi numarasına göre cari getirme işleminde hata. TaxNumber: {TaxNumber}", taxNumber);
                throw;
            }
        }

        public async Task<ServiceResult<ClientDto>> GetByTcNumberAsync(string tcNumber, CancellationToken cancellationToken)
        {
            try
            {
                var client = await _clientRepository.GetByTcNumberAsync(tcNumber, cancellationToken);

                if (client == null)
                {
                    return ServiceResult<ClientDto>.Error(
                        "Cari Bulunamadı",
                        $"{tcNumber} TC kimlik numaralı cari bulunamadı",
                        HttpStatusCode.NotFound);
                }

                return ServiceResult<ClientDto>.SuccessAsOk(client.Adapt<ClientDto>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "TC numarasına göre cari getirme işleminde hata. TcNumber: {TcNumber}", tcNumber);
                throw;
            }
        }

        public async Task<ServiceResult<DocumentCheckDto>> GetTransactionsByDocumentAsync(string documentNo, CancellationToken cancellationToken)
        {
            try
            {
                var clientTransactions = await _clientRepository.GetTransactionsByDocumentAsync(documentNo, cancellationToken);

                if (clientTransactions == null || !clientTransactions.Any())
                {
                    return ServiceResult<DocumentCheckDto>.SuccessAsOk(new DocumentCheckDto
                    {
                        IsDocumentExist = false,
                        DocumentProcessCount = 0,
                        DocumentNo = documentNo,
                        DocumentProcessList = new List<DocumentProcessList>()
                    });
                }

                return ServiceResult<DocumentCheckDto>.SuccessAsOk(clientTransactions.Adapt<DocumentCheckDto>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Doküman numarasına göre cari hareketleri getirme işleminde hata. DocumentNo: {DocumentNo}", documentNo);
                throw;
            }
        }
    }
}