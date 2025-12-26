using System.ComponentModel.DataAnnotations;

namespace VakifBankVirtualPOS.WebAPI.Options
{
    public class EmailOptions
    {
        [Required]
        public int Port { get; set; } = default!;

        [Required]
        public bool EnableSsl { get; set; } = default!;

        [Required]
        public string Host { get; set; } = default!;

        [Required]
        public EmailCredentials Credentials { get; set; } = default!;

        [Required]
        public string[] Tos { get; set; } = default!;

        public string[] RequiredCcs { get; set; } = Array.Empty<string>();
        public string[] RequiredBccs { get; set; } = Array.Empty<string>();

        [Required]
        public string ErrorTo { get; set; } = default!;
    }

    public class EmailCredentials
    {
        [Required]
        [EmailAddress]
        public string Username { get; set; } = default!;

        [Required]
        public string Password { get; set; } = default!;
    }
}