using System.Xml.Serialization;

namespace VakifBankVirtualPOS.WebAPI.Dtos.PaymentDtos
{
    /// <summary>
    /// VakıfBank Sanal POS 3D Secure Provizyon İstek DTO (XML)
    /// </summary>
    [XmlRoot("VposRequest")]
    public class VposRequestDto
    {
        // ================== ZORUNLU ALANLAR ==================

        /// <summary>
        /// Üye işyeri numarası - ZORUNLU
        /// </summary>
        public string MerchantId { get; set; }

        /// <summary>
        /// Üye işyeri şifresi - ZORUNLU
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Terminal numarası - ZORUNLU
        /// </summary>
        public string TerminalNo { get; set; }

        /// <summary>
        /// İşlem tipi (Sale) - ZORUNLU
        /// </summary>
        public string TransactionType { get; set; }

        /// <summary>
        /// İşlem kaynak tipi - ZORUNLU
        /// 0: ECommerce (3D Secure için her zaman 0)
        /// </summary>
        public string TransactionDeviceSource { get; set; }

        // ================== 3D SECURE ZORUNLU ALANLAR ==================

        /// <summary>
        /// MPI Transaction ID - ZORUNLU
        /// Callback'ten gelen VerifyEnrollmentRequestId değeri
        /// </summary>
        public string MpiTransactionId { get; set; }

        /// <summary>
        /// Electronic Commerce Indicator - ZORUNLU
        /// VISA: Y=05, A=06 | MasterCard/Troy: Y=02, A=01
        /// </summary>
        public string ECI { get; set; }

        /// <summary>
        /// Cardholder Authentication Verification Value - ZORUNLU
        /// Callback'ten gelen CAVV değeri (28 byte)
        /// </summary>
        public string CAVV { get; set; }

        // ================== OPSİYONEL ALANLAR ==================

        /// <summary>
        /// İşlem ID (benzersiz olmalı) - OPSİYONEL
        /// </summary>
        public string TransactionId { get; set; }

        [XmlIgnore]
        public bool TransactionIdSpecified => !string.IsNullOrEmpty(TransactionId);

        /// <summary>
        /// Sipariş ID - OPSİYONEL
        /// </summary>
        public string OrderId { get; set; }

        [XmlIgnore]
        public bool OrderIdSpecified => !string.IsNullOrEmpty(OrderId);

        /// <summary>
        /// Client IP adresi - OPSİYONEL
        /// </summary>
        public string ClientIp { get; set; }

        [XmlIgnore]
        public bool ClientIpSpecified => !string.IsNullOrEmpty(ClientIp);

        /// <summary>
        /// Taksit sayısı - OPSİYONEL
        /// Sadece taksitli işlemde gönderilmeli (02, 03, 04...)
        /// </summary>
        public string NumberOfInstallments { get; set; }

        [XmlIgnore]
        public bool NumberOfInstallmentsSpecified => !string.IsNullOrEmpty(NumberOfInstallments);
    }
}