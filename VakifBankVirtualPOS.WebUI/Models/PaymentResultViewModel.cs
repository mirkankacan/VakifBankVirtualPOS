namespace VakifBankVirtualPOS.WebUI.Models
{
    public class PaymentResultViewModel
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? TransactionId { get; set; }
        public string? AuthCode { get; set; }
        public string? OrderId { get; set; }
        public string? ErrorCode { get; set; }
    }
}

