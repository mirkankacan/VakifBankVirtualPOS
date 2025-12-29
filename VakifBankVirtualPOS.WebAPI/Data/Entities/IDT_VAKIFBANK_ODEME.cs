namespace VakifBankVirtualPOS.WebAPI.Data.Entities
{
    public class IDT_VAKIFBANK_ODEME
    {
        public int Id { get; set; }

        /// <summary>
        /// Sipariş numarası (Unique)
        /// </summary>
        public string OrderId { get; set; }

        /// <summary>
        /// Banka transaction ID
        /// </summary>
        public string? TransactionId { get; set; }

        /// <summary>
        /// Banka authorization kodu
        /// </summary>
        public string? AuthCode { get; set; }

        /// <summary>
        /// Ödeme tutarı
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Para birimi (949: TRY)
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// Maskelenmiş kart numarası (örn: 4506 34** **** 9828)
        /// </summary>
        public string MaskedCardNumber { get; set; }

        /// <summary>
        /// Kart sahibi adı
        /// </summary>
        public string CardHolderName { get; set; }

        /// <summary>
        /// Kart kuruluşu (VISA, MASTERCARD, TROY, AMEX)
        /// </summary>
        public string CardBrand { get; set; }

        /// <summary>
        /// Ödeme durumu (Pending, Success, Failed, Cancelled)
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Hata kodu (varsa)
        /// </summary>
        public string? ResultCode { get; set; }

        /// <summary>
        /// Hata mesajı (varsa)
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// 3D Secure durumu (Y, N, U, E, A)
        /// </summary>
        public string? ThreeDSecureStatus { get; set; }

        /// <summary>
        /// Müşteri IP adresi
        /// </summary>
        public string ClientIp { get; set; }

        /// <summary>
        /// Ödeme başlatma tarihi
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Ödeme güncelleme tarihi
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Ödeme tamamlanma tarihi
        /// </summary>
        public DateTime? CompletedAt { get; set; }

        public string? DocumentNo { get; set; }
        public string ClientCode { get; set; }
    }
}