using System.ComponentModel.DataAnnotations;

namespace VakifBankVirtualPOS.WebAPI.Options
{
    public class UiOptions
    {
        [Required]
        public string BaseUrl { get; set; } = default!;

        [Required]
        public string PaymentSuccessUrl { get; set; } = default!;

        [Required]
        public string PaymentFailureUrl { get; set; } = default!;
    }
}