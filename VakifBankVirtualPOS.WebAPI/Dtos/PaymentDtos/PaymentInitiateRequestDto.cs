using System.ComponentModel.DataAnnotations;

namespace VakifBankVirtualPOS.WebAPI.Dtos.PaymentDtos
{
    public class PaymentInitiateRequestDto
    {
        [Required(ErrorMessage = "Kart numarası zorunludur")]
        [CreditCard(ErrorMessage = "Geçerli bir kart numarası giriniz")]
        public string CardNumber { get; set; }

        [Required(ErrorMessage = "Son kullanma tarihi zorunludur")]
        public string ExpiryDate { get; set; } // YYMM formatında (örn: 2512)

        [Required(ErrorMessage = "CVV zorunludur")]
        [StringLength(4, MinimumLength = 3, ErrorMessage = "CVV 3 veya 4 haneli olmalıdır")]
        public string Cvv { get; set; }

        [Required(ErrorMessage = "Kart sahibi adı zorunludur")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Kart sahibi adı 3-100 karakter arasında olmalıdır")]
        public string CardHolderName { get; set; }

        [Required(ErrorMessage = "Tutar zorunludur")]
        [Range(0.01, 999999.99, ErrorMessage = "Tutar 0.01 ile 999999.99 arasında olmalıdır")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Cari kodu zorunludur")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Cari kodu 3-100 karakter arasında olmalıdır")]
        public string ClientCode { get; set; }

        public string? DocumentNo { get; set; }

        //[Range(1, 12, ErrorMessage = "Taksit sayısı 1 ile 12 arasında olmalıdır")]
        //public int? InstallmentCount { get; set; } = null;
    }
}