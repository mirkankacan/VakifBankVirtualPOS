using System.ComponentModel.DataAnnotations;

namespace VakifBankVirtualPOS.WebAPI.Options
{
    public class HybsOptions
    {
        [Required]
        public string Username { get; set; } = default!;

        [Required]
        public string Password { get; set; } = default!;

        [Required]
        public bool IsMobile { get; set; } = default!;
    }
}