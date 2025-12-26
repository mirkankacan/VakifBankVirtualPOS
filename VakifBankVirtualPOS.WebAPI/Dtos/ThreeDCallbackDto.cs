namespace VakifBankVirtualPOS.WebAPI.Dtos
{
    /// <summary>
    /// 3D Secure Callback'ten gelen veriler DTO
    /// Banka kullanıcıyı geri yönlendirdiğinde form post ile gelen bilgiler
    /// </summary>
    public class ThreeDCallbackDto
    {
        public string MD { get; set; }
        public string PaRes { get; set; }
        public string Status { get; set; }
        public string Eci { get; set; }
        public string Cavv { get; set; }
        public string MpiTransactionId { get; set; }
        public string VerifyEnrollmentRequestId { get; set; }
    }
}