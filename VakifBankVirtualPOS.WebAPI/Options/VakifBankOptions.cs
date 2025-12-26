using System.ComponentModel.DataAnnotations;

namespace VakifBankVirtualPOS.WebAPI.Options
{
    public class VakifBankOptions
    {
        [Required]
        public string EnrollmentUrl { get; set; } = default!;

        [Required]
        public string VposUrl { get; set; } = default!;

        [Required]
        public string MerchantId { get; set; } = default!;

        [Required]
        public string MerchantPassword { get; set; } = default!;

        [Required]
        public string TerminalNo { get; set; } = default!;

        [Required]
        public string SuccessUrl { get; set; } = default!;

        public string FailureUrl { get; set; } = default!;

        /// <summary>
        /// İşlem tipi (Sale, Auth, vb.)
        /// </summary>
        [Required]
        public string TransactionType { get; set; } = default!;

        /// <summary>
        /// İşlem kaynak tipi (0: ECommerce, 1: MailOrder)
        /// </summary>
        [Required]
        public string TransactionDeviceSource { get; set; } = default!;

        /// <summary>
        /// Para birimi (949: TRY, 840: USD, 978: EUR)
        /// </summary>
        [Required]
        public string Currency { get; set; } = default!;

        /// <summary>
        /// Cache timeout süresi (dakika)
        /// </summary>
        [Required]
        public int CacheTimeoutMinutes { get; set; } = default!;
    }
}