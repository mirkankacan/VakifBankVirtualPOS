using VakifBankVirtualPOS.WebAPI.Helpers;

namespace VakifBankVirtualPOS.WebAPI.Tools
{
    /// <summary>
    /// Configuration dosyalarını şifrelemek için tool
    /// </summary>
    public static class EncryptConfiguration
    {
        public static void Run(string[] args)
        {
            Console.WriteLine("🔐 VakıfBank Virtual POS - Configuration Şifreleme Aracı");
            Console.WriteLine("==========================================================\n");

            // Dosya yolları
            var currentDir = Directory.GetCurrentDirectory();
            var inputFile = Path.Combine(currentDir, "appsettings.Production.json");
            var outputFile = Path.Combine(currentDir, "appsettings.Production.enc");

            try
            {
                // Dosya var mı kontrol et
                if (!File.Exists(inputFile))
                {
                    Console.WriteLine($"❌ Hata: {inputFile} bulunamadı!");
                    Console.WriteLine($"📂 Mevcut dizin: {Directory.GetCurrentDirectory()}");
                    Console.WriteLine($"\n💡 İpucu: Önce appsettings.Production.json dosyasını oluşturun.");
                    return;
                }

                // Şifrele
                Console.WriteLine($"📄 Şifreleniyor: {inputFile}");
                ConfigurationEncryptionHelper.EncryptJsonFile(inputFile, outputFile);

                // Test: Çözümle
                Console.WriteLine("\n🧪 Test: Dosya çözümleniyor...");
                var decrypted = ConfigurationEncryptionHelper.DecryptJsonFile(outputFile);

                if (!string.IsNullOrEmpty(decrypted))
                {
                    Console.WriteLine("✅ Test başarılı! JSON formatı geçerli.");
                    Console.WriteLine($"📏 Çözümlenmiş içerik uzunluğu: {decrypted.Length} karakter");
                }
                else
                {
                    Console.WriteLine("❌ Test başarısız! JSON içeriği boş.");
                }

                Console.WriteLine($"\n✅ İşlem tamamlandı!");
                Console.WriteLine($"📁 Şifreli dosya: {outputFile}");
                Console.WriteLine($"\n⚠️  ÖNEMLİ NOTLAR:");
                Console.WriteLine($"   1. Bu dosya sadece bu makinede çözümlenebilir!");
                Console.WriteLine($"   2. Sunucuda yeniden şifrelemeniz gerekecek.");
                Console.WriteLine($"   3. {inputFile} dosyasını Git'e eklemeyin!");
                Console.WriteLine($"   4. Şifreli dosyayı yedekleyin!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ HATA: {ex.Message}");
                Console.WriteLine($"📋 Detay: {ex.StackTrace}");
            }

            Console.WriteLine("\nDevam etmek için bir tuşa basın...");
            Console.ReadKey();
        }
    }
}