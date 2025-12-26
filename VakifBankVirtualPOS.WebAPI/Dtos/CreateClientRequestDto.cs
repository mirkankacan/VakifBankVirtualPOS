using System.ComponentModel.DataAnnotations;

namespace VakifBankVirtualPOS.WebAPI.Dtos
{
    /// <summary>
    /// Müşteri oluşturma istek DTO
    /// </summary>
    public class CreateClientRequestDto
    {
        [Required(ErrorMessage = "Cari isim zorunludur")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "Cari isim 2-200 karakter arasında olmalıdır")]
        public required string CARI_ISIM { get; set; }

        [StringLength(20, ErrorMessage = "Vergi numarası en fazla 20 karakter olabilir")]
        public string? VERGI_NUMARASI { get; set; }

        [StringLength(100, ErrorMessage = "Vergi dairesi en fazla 100 karakter olabilir")]
        public string? VERGI_DAIRESI { get; set; }

        [StringLength(11, MinimumLength = 11, ErrorMessage = "TC Kimlik numarası 11 haneli olmalıdır")]
        public string? TCKIMLIKNO { get; set; }

        [StringLength(500, ErrorMessage = "Adres en fazla 500 karakter olabilir")]
        public string? CARI_ADRES { get; set; }

        [StringLength(50, ErrorMessage = "İl en fazla 50 karakter olabilir")]
        public string? CARI_IL { get; set; }

        [StringLength(50, ErrorMessage = "İlçe en fazla 50 karakter olabilir")]
        public string? CARI_ILCE { get; set; }

        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz")]
        [StringLength(100, ErrorMessage = "E-posta en fazla 100 karakter olabilir")]
        public string? EMAIL { get; set; }

        [StringLength(20, ErrorMessage = "Telefon en fazla 20 karakter olabilir")]
        public string? CARI_TEL { get; set; }
    }
}

