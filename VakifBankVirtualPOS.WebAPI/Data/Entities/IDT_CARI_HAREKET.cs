namespace VakifBankVirtualPOS.WebAPI.Data.Entities
{
    public class IDT_CARI_HAREKET
    {
        public int ID { get; set; }
        public string CARI_KODU { get; set; }
        public DateTime TARIH { get; set; }
        public string? BELGE_NO { get; set; }
        public string? ACIKLAMA { get; set; }
        public decimal BORC { get; set; }
        public decimal ALACAK { get; set; }
        public decimal? BAKIYE { get; set; }
        public string? HAREKET_TIPI { get; set; }
        public string? KAYIT_KULL { get; set; }
        public DateTime KAYIT_ZAMAN { get; set; }
        public int AKTARIM { get; set; }
    }
}