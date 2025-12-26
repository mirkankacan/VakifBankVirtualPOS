namespace VakifBankVirtualPOS.WebAPI.Dtos.PaymentDtos
{
    /// <summary>
    /// 3D Secure Enrollment (Kart Kayıt Kontrolü) İstek DTO
    /// </summary>
    public class EnrollmentRequestDto
    {
        public string VerifyEnrollmentRequestId { get; set; }

        public string Pan { get; set; }
        public string ExpiryDate { get; set; }
        public string PurchaseAmount { get; set; }
        public string BrandName { get; set; }
        //public string? InstallmentCount { get; set; } = null
    }
}