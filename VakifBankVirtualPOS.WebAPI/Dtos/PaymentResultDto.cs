namespace VakifBankVirtualPOS.WebAPI.Dtos
{
    /// <summary>
    /// Ödeme İşlemi Sonuç DTO
    /// Kullanıcıya döndürülen nihai sonuç
    /// </summary>
    public class PaymentResultDto
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public string TransactionId { get; set; }
        public string AuthCode { get; set; }
        public string OrderId { get; set; }
        public string ErrorCode { get; set; }
    }
}