using System.ComponentModel.DataAnnotations;

namespace VakifBankVirtualPOS.WebUI.Models
{
    public class PaymentInitiateViewModel
    {
        [Required(ErrorMessage = "Kart numarası zorunludur")]
        [Display(Name = "Kart Numarası")]
        [CreditCard(ErrorMessage = "Geçerli bir kart numarası giriniz")]
        public string CardNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Son kullanma tarihi zorunludur")]
        [Display(Name = "Son Kullanma Tarihi")]
        [RegularExpression(@"^\d{2}/\d{2}$", ErrorMessage = "Format: AA/YY (örn: 12/25)")]
        public string ExpiryDate { get; set; } = string.Empty;

        [Required(ErrorMessage = "CVV zorunludur")]
        [Display(Name = "CVV")]
        [StringLength(4, MinimumLength = 3, ErrorMessage = "CVV 3 veya 4 haneli olmalıdır")]
        public string Cvv { get; set; } = string.Empty;

        [Required(ErrorMessage = "Kart sahibi adı zorunludur")]
        [Display(Name = "Kart Sahibi Adı")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Kart sahibi adı 3-100 karakter arasında olmalıdır")]
        public string CardHolderName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tutar zorunludur")]
        [Display(Name = "Tutar")]
        [Range(0.01, 999999.99, ErrorMessage = "Tutar 0.01 ile 999999.99 arasında olmalıdır")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Cari kodu zorunludur")]
        [Display(Name = "Cari Kodu")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Cari kodu 3-100 karakter arasında olmalıdır")]
        public string ClientCode { get; set; } = string.Empty;

        [Display(Name = "Belge No")]
        public string? DocumentNo { get; set; }
    }
}

