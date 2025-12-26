using System.Xml.Serialization;

namespace VakifBankVirtualPOS.WebAPI.Dtos
{
    /// <summary>
    /// Sanal POS Provizyon Cevap DTO (XML)
    /// </summary>
    [XmlRoot("VposResponse")]
    public class VposResponseDto
    {
        public string ResultCode { get; set; }
        public string ResultDetail { get; set; }
        public string TransactionId { get; set; }
        public string OrderId { get; set; }
        public string AuthCode { get; set; }
        public string HostDate { get; set; }
        public string CurrencyAmount { get; set; }
        public string Rrn { get; set; }
        public string Stan { get; set; }
        public string BatchNo { get; set; }
        public string HostReferenceNumber { get; set; }
    }
}