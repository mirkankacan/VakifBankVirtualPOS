using System.Security.Cryptography;
using System.Text;

namespace VakifBankVirtualPOS.WebAPI.Helpers
{
    /// <summary>
    /// Configuration dosyaları için şifreleme/çözme helper sınıfı
    /// </summary>
    public static class ConfigurationEncryptionHelper
    {
        private const string EnvVarName = "EGESEHIR_VIRTUAL_POS_ENCRYPTION_KEY";

        /// <summary>
        /// Entropy değerini environment variable'dan veya default'tan alır
        /// </summary>
        private static byte[] GetEntropy()
        {
            var entropyKey = Environment.GetEnvironmentVariable(EnvVarName);

            if (string.IsNullOrEmpty(entropyKey))
            {
                throw new InvalidOperationException(
                    $"❌ KRİTİK HATA: Environment variable '{EnvVarName}' tanımlanmamış!\n\n" +
                    $"Çözüm:\n" +
                    $"PowerShell (Administrator):\n" +
                    $"[Environment]::SetEnvironmentVariable(\"{EnvVarName}\", \"YourSecureKey123!\", \"Machine\")\n" +
                    $"iisreset\n\n" +
                    $"veya User seviyesinde (Development):\n" +
                    $"[Environment]::SetEnvironmentVariable(\"{EnvVarName}\", \"YourSecureKey123!\", \"User\")\n"
                );
            }

            Console.WriteLine($"✅ Encryption key environment variable'dan yüklendi.");
            return Encoding.UTF8.GetBytes(entropyKey);
        }

        /// <summary>
        /// JSON dosyasını şifreler
        /// </summary>
        public static void EncryptJsonFile(string inputPath, string outputPath)
        {
            try
            {
                if (!File.Exists(inputPath))
                    throw new FileNotFoundException($"Dosya bulunamadı: {inputPath}");

                var entropy = GetEntropy();

                // Dosyayı oku
                var jsonContent = File.ReadAllText(inputPath);
                var plainBytes = Encoding.UTF8.GetBytes(jsonContent);

                // Şifrele (Windows DPAPI)
                var encryptedBytes = ProtectedData.Protect(
                    plainBytes,
                    entropy,
                    DataProtectionScope.LocalMachine
                );

                // Base64'e çevir ve kaydet
                var base64 = Convert.ToBase64String(encryptedBytes);

                // Klasör yoksa oluştur
                var directory = Path.GetDirectoryName(outputPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.WriteAllText(outputPath, base64);

                Console.WriteLine($"✅ Dosya şifrelendi: {outputPath}");
                Console.WriteLine($"📏 Orijinal boyut: {plainBytes.Length} bytes");
                Console.WriteLine($"📏 Şifreli boyut: {base64.Length} bytes");
            }
            catch (Exception ex)
            {
                throw new Exception($"Şifreleme hatası: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Şifrelenmiş dosyayı çözümler
        /// </summary>
        public static string DecryptJsonFile(string encryptedPath)
        {
            try
            {
                if (!File.Exists(encryptedPath))
                    throw new FileNotFoundException($"Şifreli dosya bulunamadı: {encryptedPath}");

                var entropy = GetEntropy();

                // Base64'ten byte array'e
                var base64 = File.ReadAllText(encryptedPath);
                var encryptedBytes = Convert.FromBase64String(base64);

                // Şifreyi çöz
                var decryptedBytes = ProtectedData.Unprotect(
                    encryptedBytes,
                    entropy,
                    DataProtectionScope.LocalMachine
                );

                // JSON string'e çevir
                var json = Encoding.UTF8.GetString(decryptedBytes);

                Console.WriteLine($"✅ Dosya çözümlendi: {encryptedPath}");

                return json;
            }
            catch (CryptographicException ex)
            {
                throw new Exception(
                    "Şifre çözme hatası! Bu dosya farklı bir makinede şifrelenmiş olabilir veya encryption key hatalı.",
                    ex
                );
            }
            catch (Exception ex)
            {
                throw new Exception($"Çözümleme hatası: {ex.Message}", ex);
            }
        }
    }
}