using VakifBankVirtualPOS.WebAPI.Common;
using VakifBankVirtualPOS.WebAPI.Dtos;

namespace VakifBankVirtualPOS.WebAPI.Services.Interfaces
{
    /// <summary>
    /// Müşteri işlemleri servisi interface
    /// </summary>
    public interface IClientService
    {
        /// <summary>
        /// Yeni müşteri oluşturur
        /// </summary>
        /// <param name="request">Müşteri oluşturma isteği</param>
        /// <param name="cancellationToken">İptal token</param>
        /// <returns>Oluşturulan müşteri bilgileri</returns>
        Task<ServiceResult<ClientResponseDto>> CreateClientAsync(CreateClientRequestDto request, CancellationToken cancellationToken);

        /// <summary>
        /// Vergi numarasına göre müşteri getirir
        /// </summary>
        /// <param name="taxNumber">Vergi numarası</param>
        /// <param name="cancellationToken">İptal token</param>
        /// <returns>Müşteri bilgileri</returns>
        Task<ServiceResult<ClientResponseDto>> GetByTaxNumberAsync(string taxNumber, CancellationToken cancellationToken);

        /// <summary>
        /// TC kimlik numarasına göre müşteri getirir
        /// </summary>
        /// <param name="tcNumber">TC kimlik numarası</param>
        /// <param name="cancellationToken">İptal token</param>
        /// <returns>Müşteri bilgileri</returns>
        Task<ServiceResult<ClientResponseDto>> GetByTcNumberAsync(string tcNumber, CancellationToken cancellationToken);
    }
}

