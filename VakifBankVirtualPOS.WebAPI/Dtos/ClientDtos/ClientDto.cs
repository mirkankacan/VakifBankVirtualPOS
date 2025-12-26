namespace VakifBankVirtualPOS.WebAPI.Dtos.ClientDtos
{
    /// <summary>
    /// Müşteri yanıt DTO
    /// </summary>
    public class ClientDto
    {
        public int ID { get; set; }
        public string CARI_KOD { get; set; }
        public string CARI_ISIM { get; set; }
        public string? VERGI_DAIRESI { get; set; }
        public string? VERGI_NUMARASI { get; set; }
        public string? TCKIMLIKNO { get; set; }
        public string? CARI_ADRES { get; set; }
        public string? CARI_IL { get; set; }
        public string? CARI_ILCE { get; set; }
        public string? EMAIL { get; set; }
        public string? CARI_TEL { get; set; }
        public float? BAKIYE { get; set; }
        public string? SUBE_CARI_KOD { get; set; }
    }
}