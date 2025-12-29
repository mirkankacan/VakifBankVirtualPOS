using System.Text.Json;
using VakifBankVirtualPOS.WebAPI.Helpers;

namespace VakifBankVirtualPOS.WebAPI.Configuration
{
    /// <summary>
    /// Şifrelenmiş JSON dosyasından configuration yükleyen provider
    /// </summary>
    public class EncryptedJsonConfigurationProvider : ConfigurationProvider
    {
        private readonly string _filePath;

        public EncryptedJsonConfigurationProvider(string filePath)
        {
            _filePath = filePath;
        }

        public override void Load()
        {
            try
            {
                if (!File.Exists(_filePath))
                {
                    Console.WriteLine($"⚠️  Şifreli config dosyası bulunamadı: {_filePath}");
                    return;
                }

                // Şifreyi çöz
                var json = ConfigurationEncryptionHelper.DecryptJsonFile(_filePath);

                // JSON'u parse et ve Data dictionary'ye ekle
                using var doc = JsonDocument.Parse(json);
                Data = ParseJsonElement(doc.RootElement);

                Console.WriteLine($"✅ Şifreli config yüklendi: {_filePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Şifreli config yüklenirken hata: {ex.Message}");
                throw;
            }
        }

        private Dictionary<string, string> ParseJsonElement(JsonElement element, string prefix = "")
        {
            var data = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            if (element.ValueKind == JsonValueKind.Object)
            {
                foreach (var property in element.EnumerateObject())
                {
                    var key = string.IsNullOrEmpty(prefix)
                        ? property.Name
                        : $"{prefix}:{property.Name}";

                    var childData = ParseJsonElement(property.Value, key);
                    foreach (var kvp in childData)
                    {
                        data[kvp.Key] = kvp.Value;
                    }
                }
            }
            else if (element.ValueKind == JsonValueKind.Array)
            {
                int index = 0;
                foreach (var item in element.EnumerateArray())
                {
                    var key = $"{prefix}:{index}";
                    var childData = ParseJsonElement(item, key);
                    foreach (var kvp in childData)
                    {
                        data[kvp.Key] = kvp.Value;
                    }
                    index++;
                }
            }
            else
            {
                data[prefix] = element.ToString();
            }

            return data;
        }
    }

    /// <summary>
    /// Şifrelenmiş JSON configuration source
    /// </summary>
    public class EncryptedJsonConfigurationSource : IConfigurationSource
    {
        private readonly string _filePath;

        public EncryptedJsonConfigurationSource(string filePath)
        {
            _filePath = filePath;
        }

        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new EncryptedJsonConfigurationProvider(_filePath);
        }
    }

    /// <summary>
    /// Extension methods for adding encrypted JSON configuration
    /// </summary>
    public static class EncryptedJsonConfigurationExtensions
    {
        public static IConfigurationBuilder AddEncryptedJsonFile(
            this IConfigurationBuilder builder,
            string path)
        {
            return builder.Add(new EncryptedJsonConfigurationSource(path));
        }
    }
}