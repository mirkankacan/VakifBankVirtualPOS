using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace VakifBankVirtualPOS.WebAPI.Helpers
{
    /// <summary>
    /// XML işlemleri helper sınıfı
    /// </summary>
    public static class XmlHelper
    {
        /// <summary>
        /// Object'i XML string'e serialize eder (null değerleri atlar)
        /// </summary>
        public static string SerializeToXml<T>(T obj) where T : class
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            // XmlSerializer ile serialize et
            var xmlSerializer = new XmlSerializer(typeof(T));
            using var stringWriter = new StringWriter();
            using var xmlWriter = XmlWriter.Create(stringWriter, new XmlWriterSettings
            {
                Indent = false,
                OmitXmlDeclaration = false,
                Encoding = System.Text.Encoding.UTF8
            });

            // Null namespace ile serialize et
            var namespaces = new XmlSerializerNamespaces();
            namespaces.Add(string.Empty, string.Empty);

            xmlSerializer.Serialize(xmlWriter, obj, namespaces);

            var xmlString = stringWriter.ToString();

            // Boş elementleri kaldır
            return RemoveEmptyElements(xmlString);
        }

        /// <summary>
        /// XML'den boş elementleri kaldırır
        /// </summary>
        private static string RemoveEmptyElements(string xmlString)
        {
            var xdoc = XDocument.Parse(xmlString);

            // Boş veya sadece whitespace içeren elementleri bul ve kaldır
            xdoc.Descendants()
                .Where(e => string.IsNullOrWhiteSpace(e.Value) && !e.HasElements)
                .Remove();

            return xdoc.ToString(SaveOptions.DisableFormatting);
        }

        /// <summary>
        /// XML string'i object'e deserialize eder
        /// </summary>
        public static T DeserializeFromXml<T>(string xmlString) where T : class
        {
            if (string.IsNullOrWhiteSpace(xmlString))
                throw new ArgumentException("XML string boş olamaz", nameof(xmlString));

            var xmlSerializer = new XmlSerializer(typeof(T));
            using var stringReader = new StringReader(xmlString);
            return (T)xmlSerializer.Deserialize(stringReader);
        }

        /// <summary>
        /// XML string'den belirli bir elementi okur
        /// </summary>
        public static string GetElementValue(string xmlString, string elementName)
        {
            if (string.IsNullOrWhiteSpace(xmlString))
                throw new ArgumentException("XML string boş olamaz", nameof(xmlString));
            if (string.IsNullOrWhiteSpace(elementName))
                throw new ArgumentException("Element adı boş olamaz", nameof(elementName));

            try
            {
                var xdoc = XDocument.Parse(xmlString);
                var element = xdoc.Descendants(elementName).FirstOrDefault();
                return element?.Value ?? string.Empty;
            }
            catch (XmlException ex)
            {
                throw new InvalidOperationException($"XML parse hatası: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// XML string'in geçerli olup olmadığını kontrol eder
        /// </summary>
        public static bool IsValidXml(string xmlString)
        {
            if (string.IsNullOrWhiteSpace(xmlString))
                return false;

            try
            {
                XDocument.Parse(xmlString);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}

