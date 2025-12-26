namespace VakifBankVirtualPOS.WebAPI.Dtos.HybsDtos
{
    public class TokenResult
    {
        public int buyuksehirid { get; set; }
        public string buyuksehiradi { get; set; }
        public int ilcebelediyeid { get; set; }
        public int ilid { get; set; }
        public string il { get; set; }
        public int userid { get; set; }
        public string username { get; set; }
        public string namelastname { get; set; }
        public string cariHesapTuru { get; set; }
        public string email { get; set; }
        public int[] usergroup { get; set; }
        public string authtoken { get; set; }
        public DateTime issuedOn { get; set; }
        public DateTime expiredOn { get; set; }
        public object logo { get; set; }
        public string depolamaAlanAdi { get; set; }
        public int tasimaizinsureleri_yil { get; set; }
        public object betontasimaizinsureleri_yil { get; set; }
        public int tasimaizinsureleri_ay { get; set; }
        public int tasimaizinsureleri_gun { get; set; }
        public int depolamaalaniid { get; set; }
        public int depolamaalanisahaisletmeciid { get; set; }
        public object firmaid { get; set; }
        public bool taahhutnameonay { get; set; }
        public object gsm { get; set; }
    }
}