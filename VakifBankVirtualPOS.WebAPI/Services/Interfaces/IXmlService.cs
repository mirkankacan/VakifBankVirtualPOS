namespace VakifBankVirtualPOS.WebAPI.Services.Interfaces
{
    /// <summary>
    /// XML işlemleri servisi interface
    /// </summary>
    public interface IXmlService
    {
        /// <summary>
        /// Object'i XML string'e serialize eder
        /// </summary>
        /// <typeparam name="T">Serialize edilecek tip</typeparam>
        /// <param name="obj">Serialize edilecek object</param>
        /// <returns>XML string</returns>
        string SerializeToXml<T>(T obj) where T : class;

        /// <summary>
        /// XML string'i object'e deserialize eder
        /// </summary>
        /// <typeparam name="T">Deserialize edilecek tip</typeparam>
        /// <param name="xmlString">XML string</param>
        /// <returns>Deserialize edilmiş object</returns>
        T DeserializeFromXml<T>(string xmlString) where T : class;

        /// <summary>
        /// XML string'den belirli bir elementi okur
        /// </summary>
        /// <param name="xmlString">XML string</param>
        /// <param name="elementName">Element adı</param>
        /// <returns>Element değeri</returns>
        string GetElementValue(string xmlString, string elementName);

        /// <summary>
        /// XML string'in geçerli olup olmadığını kontrol eder
        /// </summary>
        /// <param name="xmlString">XML string</param>
        /// <returns>Geçerli ise true</returns>
        bool IsValidXml(string xmlString);
    }
}