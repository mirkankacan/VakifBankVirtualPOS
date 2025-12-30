using System.ComponentModel.DataAnnotations;

namespace VakifBankVirtualPOS.WebUI.Options
{
    public class ApiOptions
    {
        [Required]
        public string BaseUrl { get; set; } = default!;

        [Required]
        public string ApiKey { get; set; } = default!;
    }
}