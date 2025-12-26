namespace VakifBankVirtualPOS.WebAPI.Dtos.HybsDtos
{
    public class ClientDetailDto
    {
        public int FirmaId { get; set; }
        public int FirmaTipiId { get; set; }
        public string FirmaTipi { get; set; }
        public string FirmaAdi { get; set; }
        public string IlAdi { get; set; }
        public string IlceAdi { get; set; }
        public string Adres { get; set; }
        public string Eposta { get; set; }
        public string SicilNo { get; set; }
        public string GSM1 { get; set; }
        public string VergiDairesi { get; set; }
        public string VergiNo { get; set; }
        public string Aciklama { get; set; }
    }
}