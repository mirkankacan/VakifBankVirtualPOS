namespace VakifBankVirtualPOS.WebUI.Models.PaymentViewModels
{
    /// <summary>
    /// 3D Secure Enrollment (Kart Kayıt Kontrolü) Cevap ViewModel
    /// </summary>
    public class EnrollmentResponseViewModel
    {
        public string OrderId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string MessageErrorCode { get; set; } = string.Empty;
        public string ACSUrl { get; set; } = string.Empty;
        public string PAReq { get; set; } = string.Empty;
        public string TermUrl { get; set; } = string.Empty;
        public string MD { get; set; } = string.Empty;
    }
}

