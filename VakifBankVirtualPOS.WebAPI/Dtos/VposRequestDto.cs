using System.Xml.Serialization;

namespace VakifBankVirtualPOS.WebAPI.Dtos
{
    /// <summary>
    /// Sanal POS Provizyon İstek DTO (XML)
    /// </summary>
    [XmlRoot("VposRequest")]
    public class VposRequestDto
    {
        public string MerchantId { get; set; }
        public string Password { get; set; }
        public string TransactionType { get; set; }
        public string TransactionId { get; set; }
        public string TerminalNo { get; set; }
        public string CurrencyAmount { get; set; }
        public string CurrencyCode { get; set; }
        public string NumberOfInstallments { get; set; }
        public string Pan { get; set; }
        public string Expiry { get; set; }
        public string Cvv { get; set; }
        public string ClientIp { get; set; }
        public string OrderId { get; set; }
        public string TransactionDeviceSource { get; set; }

        // 3D Secure alanları
        public string ECI { get; set; }

        public string CAVV { get; set; }
        public string MpiTransactionId { get; set; }
    }
}