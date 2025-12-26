using VakifBankVirtualPOS.WebAPI.Services.Interfaces;

namespace VakifBankVirtualPOS.WebAPI.Services.Implementations
{
    /// <summary>
    /// HTTP istek servisi
    /// </summary>
    public class HttpClientService : IHttpClientService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public HttpClientService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        }

        /// <summary>
        /// Form data ile POST isteği gönderir
        /// </summary>
        public async Task<string> PostFormDataAsync(string url, Dictionary<string, string> formData, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentException("URL boş olamaz", nameof(url));

            if (formData == null || formData.Count == 0)
                throw new ArgumentException("Form data boş olamaz", nameof(formData));

            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                var content = new FormUrlEncodedContent(formData);

                var response = await httpClient.PostAsync(url, content, cancellationToken);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadAsStringAsync(cancellationToken);
            }
            catch (HttpRequestException ex)
            {
                throw;
            }
        }

        /// <summary>
        /// XML string ile POST isteği gönderir (prmstr parametresi ile)
        /// </summary>
        public async Task<string> PostXmlAsync(string url, string xmlContent, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentException("URL boş olamaz", nameof(url));

            if (string.IsNullOrWhiteSpace(xmlContent))
                throw new ArgumentException("XML içeriği boş olamaz", nameof(xmlContent));

            try
            {
                var httpClient = _httpClientFactory.CreateClient();

                // VakıfBank'ın beklediği format: prmstr parametresi
                var formData = new Dictionary<string, string>
                {
                    { "prmstr", xmlContent }
                };

                var content = new FormUrlEncodedContent(formData);

                var response = await httpClient.PostAsync(url, content, cancellationToken);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadAsStringAsync(cancellationToken);
            }
            catch (HttpRequestException ex)
            {
                throw;
            }
        }
    }
}