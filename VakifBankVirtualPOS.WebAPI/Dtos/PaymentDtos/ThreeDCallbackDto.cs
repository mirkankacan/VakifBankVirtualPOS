namespace VakifBankVirtualPOS.WebAPI.Dtos.PaymentDtos
{
    /// <summary>
    /// 3D Secure Callback'ten gelen veriler DTO
    /// Banka kullanıcıyı geri yönlendirdiğinde form post ile gelen bilgiler
    /// </summary>
    public class ThreeDCallbackDto
    {
        public string MerchantId { get; set; }
        public string VerifyEnrollmentRequestId { get; set; }
        public string PurchAmount { get; set; }
        public string PurchCurrency { get; set; }
        public string Xid { get; set; }
        public string SessionInfo { get; set; } // OrderId burada
        public string Status { get; set; } // Y, A, U, E, N
        public string CAVV { get; set; }
        public string ECI { get; set; }
        public string ExpiryDate { get; set; }
        public string InstallmentCount { get; set; }
        public string MdStatus { get; set; } // 0, 1, 7
    }
}