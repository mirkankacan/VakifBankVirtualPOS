namespace VakifBankVirtualPOS.WebAPI.Services.Interfaces
{
    /// <summary>
    /// HTTP istek servisi interface
    /// </summary>
    public interface IHttpClientService
    {
        /// <summary>
        /// Form data ile POST isteği gönderir
        /// </summary>
        /// <param name="url">Hedef URL</param>
        /// <param name="formData">Form verileri</param>
        /// <returns>Response string</returns>
        Task<string> PostFormDataAsync(string url, Dictionary<string, string> formData, CancellationToken cancellationToken);

        /// <summary>
        /// XML string ile POST isteği gönderir
        /// </summary>
        /// <param name="url">Hedef URL</param>
        /// <param name="xmlContent">XML içeriği</param>
        /// <returns>Response string</returns>
        Task<string> PostXmlAsync(string url, string xmlContent, CancellationToken cancellationToken);
    }
}