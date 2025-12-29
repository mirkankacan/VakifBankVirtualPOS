namespace VakifBankVirtualPOS.WebUI.Models.ClientViewModels
{
    /// <summary>
    /// Müşteri ViewModel
    /// </summary>
    public class ClientViewModel
    {
        public int ID { get; set; }
        public string CARI_KOD { get; set; } = string.Empty;
        public string CARI_ISIM { get; set; } = string.Empty;
    }
}

