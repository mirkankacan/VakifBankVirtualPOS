namespace VakifBankVirtualPOS.WebAPI.Dtos
{
    /// <summary>
    /// 3D Secure Enrollment (Kart Kayıt Kontrolü) Cevap DTO
    /// </summary>
    public class EnrollmentResponseDto
    {
        public string OrderId { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
        public string MessageErrorCode { get; set; }
        public string ACSUrl { get; set; }
        public string PAReq { get; set; }
        public string TermUrl { get; set; }
        public string MD { get; set; }
    }
}