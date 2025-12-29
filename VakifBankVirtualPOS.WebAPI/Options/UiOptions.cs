using System.ComponentModel.DataAnnotations;

namespace VakifBankVirtualPOS.WebAPI.Options
{
    public class UiOptions
    {
        [Required]
        public string BaseUrl { get; set; } = default!;

        [Required]
        public string SuccessUrl { get; set; } = default!;

        [Required]
        public string FailureUrl { get; set; } = default!;
    }
}