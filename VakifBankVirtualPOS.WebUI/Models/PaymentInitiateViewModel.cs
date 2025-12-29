using System.ComponentModel.DataAnnotations;

namespace VakifBankVirtualPOS.WebUI.Models
{
    public class PaymentInitiateViewModel
    {
        [Required(ErrorMessage = "Kart numarası zorunludur")]
        [CreditCard(ErrorMessage = "Geçerli bir kart numarası giriniz")]
        public string CardNumber { get; set; } = default!;

        [Required(ErrorMessage = "Son kullanma tarihi zorunludur")]
        [RegularExpression(@"^\d{2}/\d{2}$", ErrorMessage = "Format: AA/YY (örn: 12/25)")]
        public string ExpiryDate { get; set; } = default!;

        [Required(ErrorMessage = "CVV zorunludur")]
        [StringLength(4, MinimumLength = 3, ErrorMessage = "CVV 3 veya 4 haneli olmalıdır")]
        public string Cvv { get; set; } = default!;

        [Required(ErrorMessage = "Kart sahibi adı zorunludur")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Kart sahibi adı 3-100 karakter arasında olmalıdır")]
        public string CardHolderName { get; set; } = default!;

        [Required(ErrorMessage = "Tutar zorunludur")]
        [Range(0.01, 999999.99, ErrorMessage = "Tutar 0.01 ile 999999.99 arasında olmalıdır")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Cari kodu zorunludur")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Cari kodu 3-100 karakter arasında olmalıdır")]
        public string ClientCode { get; set; } = default!;

        [Display(Name = "Belge No")]
        public string? DocumentNo { get; set; }
    }
}