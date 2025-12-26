using VakifBankVirtualPOS.WebAPI.Common;
using VakifBankVirtualPOS.WebAPI.Dtos;

namespace VakifBankVirtualPOS.WebAPI.Services.Interfaces
{
    /// <summary>
    /// VakıfBank Sanal POS ödeme servisi interface
    /// </summary>
    public interface IVakifBankPaymentService
    {
        /// <summary>
        /// 3D Secure işlemini başlatır (Enrollment)
        /// </summary>
        /// <param name="request">Ödeme başlatma isteği</param>
        /// <param name="clientIp">Kullanıcının IP adresi</param>
        /// <returns>Enrollment cevabı (ACS URL, PAReq, vb.)</returns>
        Task<ServiceResult<EnrollmentResponseDto>> InitiateThreeDSecureAsync(PaymentInitiateRequestDto request, CancellationToken cancellationToken);

        /// <summary>
        /// 3D Secure doğrulama sonrası ödemeyi tamamlar
        /// </summary>
        /// <param name="callback">Bankadan gelen callback verileri</param>
        /// <param name="clientIp">Kullanıcının IP adresi</param>
        /// <returns>Ödeme sonucu</returns>
        Task<ServiceResult<PaymentResultDto>> CompletePaymentAsync(ThreeDCallbackDto callback, CancellationToken cancellationToken);
    }
}