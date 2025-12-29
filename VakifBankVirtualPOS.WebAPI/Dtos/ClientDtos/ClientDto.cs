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
    }
}