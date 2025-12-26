using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using VakifBankVirtualPOS.WebAPI.Services.Interfaces;

namespace VakifBankVirtualPOS.WebAPI.Services.Implementations
{
    /// <summary>
    /// XML işlemleri servisi
    /// </summary>
    public class XmlService : IXmlService
    {
        /// <summary>
        /// Object'i XML string'e serialize eder
        /// </summary>
        public string SerializeToXml<T>(T obj) where T : class
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            var xmlSerializer = new XmlSerializer(typeof(T));
            using var stringWriter = new StringWriter();
            using var xmlWriter = XmlWriter.Create(stringWriter, new XmlWriterSettings
            {
                Indent = false,
                OmitXmlDeclaration = false,
                Encoding = System.Text.Encoding.UTF8
            });

            xmlSerializer.Serialize(xmlWriter, obj);
            return stringWriter.ToString();
        }

        /// <summary>
        /// XML string'i object'e deserialize eder
        /// </summary>
        public T DeserializeFromXml<T>(string xmlString) where T : class
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
        public string GetElementValue(string xmlString, string elementName)
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
        public bool IsValidXml(string xmlString)
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