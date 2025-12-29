using System.Security.Cryptography;
using System.Text;

namespace VakifBankVirtualPOS.WebAPI.Helpers
{
    public static class ConfigurationEncryptionHelper
    {
        private static readonly byte[] _entropy = Encoding.UTF8.GetBytes("VakifBankEgesehir2024!SecureKey#POS");

        /// <summary>
        /// JSON dosyasını şifreler
        /// </summary>
        public static void EncryptJsonFile(string inputPath, string outputPath)
        {
            try
            {
                if (!File.Exists(inputPath))
                    throw new FileNotFoundException($"Dosya bulunamadı: {inputPath}");

                // Dosyayı oku
                var jsonContent = File.ReadAllText(inputPath);
                var plainBytes = Encoding.UTF8.GetBytes(jsonContent);

                // Şifrele (Windows DPAPI)
                var encryptedBytes = ProtectedData.Protect(
                    plainBytes,
                    _entropy,
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

                // Base64'ten byte array'e
                var base64 = File.ReadAllText(encryptedPath);
                var encryptedBytes = Convert.FromBase64String(base64);

                // Şifreyi çöz
                var decryptedBytes = ProtectedData.Unprotect(
                    encryptedBytes,
                    _entropy,
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
                    "Şifre çözme hatası! Bu dosya farklı bir makinede şifrelenmiş olabilir. " +
                    "DataProtectionScope.LocalMachine ile şifrelenen dosyalar sadece aynı makinede çözümlenebilir.",
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