namespace VakifBankVirtualPOS.WebUI.Models.PaymentViewModels
{
    /// <summary>
    /// Ödeme İşlemi Sonuç ViewModel
    /// </summary>
    public class PaymentResultViewModel
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public string OrderId { get; set; }
        public string ResultCode { get; set; }
        public string TransactionId { get; set; }
        public string AuthCode { get; set; }
    }
}