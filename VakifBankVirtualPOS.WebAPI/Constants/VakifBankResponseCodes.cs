namespace VakifBankVirtualPOS.WebAPI.Constants
{
    /// <summary>
    /// VakıfBank Sanal POS cevap kodları
    /// Kaynak: VakıfBank Sanal POS Entegrasyon Dökümanı v2.5
    /// </summary>
    public static class VakifBankResponseCodes
    {
        private static readonly Dictionary<string, string> _codes = new()
        {
            // ============================================
            // BAŞARILI İŞLEMLER
            // ============================================
            { "0000", "İşleminiz başarıyla tamamlandı" },

            // ============================================
            // KART VE HESAP HATALARI (00XX - 00XX)
            // ============================================
            { "0001", "Lütfen kartınızı veren banka ile iletişime geçin" },
            { "0002", "Lütfen kartınızı veren banka ile iletişime geçin (Özel Durum)" },
            { "0003", "İşyeri hatası. Lütfen daha sonra tekrar deneyin" },
            { "0004", "Kartınızla ilgili bir sorun var. Lütfen bankanızla iletişime geçin" },
            { "0005", "İşleminiz bankanız tarafından onaylanmadı" },
            { "0006", "Bir hata oluştu. Lütfen daha sonra tekrar deneyin" },
            { "0007", "Kartınızla ilgili bir sorun var. Lütfen bankanızla iletişime geçin (Özel Durum)" },
            { "0012", "Geçersiz işlem. Lütfen bilgilerinizi kontrol edin" },
            { "0013", "Geçersiz tutar. Lütfen tutarı kontrol edin" },
            { "0014", "Kart numarası hatalı. Lütfen kontrol edip tekrar deneyin" },
            { "0015", "Kartınızı veren banka sistemde bulunamadı" },
            { "0021", "İşlem iptal edilemedi" },
            { "0025", "İşlem kaydı bulunamadı" },
            { "0028", "Sistem geçici olarak kullanılamıyor. Lütfen daha sonra tekrar deneyin" },
            { "0030", "Sistem hatası oluştu. Lütfen daha sonra tekrar deneyin" },
            { "0033", "Kartınızın son kullanma tarihi geçmiş" },
            { "0034", "Güvenlik nedeniyle işlem gerçekleştirilemedi (Sahtecilik şüphesi)" },
            { "0036", "Kartınız kısıtlı. Lütfen bankanızla iletişime geçin" },
            { "0037", "Güvenlik nedeniyle işlem gerçekleştirilemedi. Lütfen bankanızla iletişime geçin" },
            { "0038", "PIN deneme sayısı aşıldı. Kartınız bloke olmuş olabilir" },
            { "0041", "Kart kayıp olarak bildirilmiş. Lütfen bankanızla iletişime geçin" },
            { "0043", "Kart çalıntı olarak bildirilmiş. Lütfen bankanızla iletişime geçin" },
            { "0051", "Yetersiz bakiye veya limit. Lütfen farklı bir kart deneyin" },
            { "0052", "Hesap bulunamadı. Lütfen bankanızla iletişime geçin" },
            { "0053", "Hesap bulunamadı. Lütfen bankanızla iletişime geçin" },
            { "0054", "Kartınızın son kullanma tarihi geçmiş. Lütfen güncel bir kart kullanın" },
            { "0055", "PIN hatalı. Lütfen tekrar deneyin" },
            { "0056", "Kart tanımlı değil. Lütfen bankanızla iletişime geçin" },
            { "0057", "Bu işlem kartınız için yapılamıyor" },
            { "0058", "Bu terminal üzerinden işlem yapılamıyor" },
            { "0059", "Sahtecilik şüphesi. Lütfen bankanızla iletişime geçin" },
            { "0061", "İşlem limitleri aşıldı. Lütfen daha düşük bir tutar deneyin" },
            { "0062", "Kartınız kısıtlı. Lütfen bankanızla iletişime geçin" },
            { "0063", "Güvenlik ihlali. İşlem gerçekleştirilemedi" },
            { "0065", "Günlük işlem adedi aşıldı. Lütfen yarın tekrar deneyin" },
            { "0075", "PIN deneme sayısı aşıldı" },
            { "0076", "İşlem reddedildi. Lütfen bankanızla iletişime geçin" },
            { "0077", "Kart bilgileri hatalı. Lütfen kontrol edin" },
            { "0078", "Kartınız aktif değil. Lütfen bankanızla iletişime geçin" },
            { "0089", "Kimlik doğrulama hatası" },

            // ============================================
            // SİSTEM VE İLETİŞİM HATALARI (00XX)
            // ============================================
            { "0091", "Bankanızın sistemi geçici olarak kullanılamıyor. Lütfen daha sonra tekrar deneyin" },
            { "0092", "Bağlantı hatası. Lütfen daha sonra tekrar deneyin" },
            { "0093", "İşlem tamamlanamadı. Lütfen daha sonra tekrar deneyin" },
            { "0096", "Sistem arızası. Lütfen daha sonra tekrar deneyin" },

            // ============================================
            // 3D SECURE HATALARI (ÖZEL KODLAR)
            // ============================================
            { "1001", "3D Secure doğrulama başarısız" },
            { "1006", "3D Secure şifresi hatalı" },
            { "1007", "3D Secure doğrulama zaman aşımına uğradı" },
            { "1009", "3D Secure kayıt bulunamadı" },
            { "1010", "3D Secure işlemi iptal edildi" },
            { "1011", "3D Secure sistemi geçici olarak kullanılamıyor" },
            { "1012", "3D Secure kart 3D programına kayıtlı değil" },
            { "1013", "3D Secure doğrulama hatası" },
            { "1014", "3D Secure sistem hatası" },
            { "1015", "3D Secure güvenlik ihlali" },

            // ============================================
            // VAKIFBANK ÖZEL HATA KODLARI
            // ============================================
            { "5001", "İşlem onaylanmadı. Lütfen bankanızla iletişime geçin" },
            { "5002", "Geçersiz işlem" },
            { "5003", "Geçersiz üye işyeri numarası" },
            { "5004", "İşlem iptal edildi" },
            { "5005", "İşlem reddedildi" },
            { "5006", "Hatalı terminal numarası" },
            { "5007", "Geçersiz şifre" },
            { "5008", "Geçersiz işlem tipi" },
            { "5009", "Geçersiz tutar" },
            { "5010", "İşlem zaten mevcut" },
            { "5011", "İşlem bulunamadı" },
            { "5012", "İşlem zaman aşımına uğradı" },
            { "5013", "Çift işlem" },
            { "5014", "Format hatası" },
            { "5015", "Geçersiz para birimi" },
            { "5016", "İade tutarı orijinal tutarı aşıyor" },
            { "5017", "İşlem iade edilemez" },
            { "5018", "İptal edilemez işlem" },
            { "5019", "İşlem kapandı" },
            { "5020", "Batch kapalı" },

            // ============================================
            // PROVIZYON HATALARI
            // ============================================
            { "8001", "Provizyon alınamadı. Lütfen bankanızla iletişime geçin" },
            { "8002", "Provizyon reddedildi" },
            { "8003", "Provizyon onaylanmadı" },
            { "8004", "Provizyon hatası" },

            // ============================================
            // GENEL HATALAR
            // ============================================
            { "9001", "Genel sistem hatası" },
            { "9002", "Veritabanı hatası" },
            { "9003", "Bağlantı hatası" },
            { "9004", "Zaman aşımı" },
            { "9999", "Tanımlanamayan hata" },
        };

        /// <summary>
        /// Koda göre kullanıcı mesajını getirir
        /// </summary>
        /// <param name="code">Response code (örn: "0054")</param>
        /// <returns>Kullanıcıya gösterilecek mesaj</returns>
        public static string GetMessage(string? code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return "Bir hata oluştu. Lütfen daha sonra tekrar deneyin";

            return _codes.TryGetValue(code, out var message)
                ? message
                : $"Bir hata oluştu (Kod: {code}). Lütfen daha sonra tekrar deneyin";
        }

        /// <summary>
        /// Kodun başarılı olup olmadığını kontrol eder
        /// </summary>
        /// <param name="code">Response code</param>
        /// <returns>Başarılı ise true</returns>
        public static bool IsSuccess(string? code)
        {
            return code == "0000";
        }
    }
}