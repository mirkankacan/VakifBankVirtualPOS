namespace VakifBankVirtualPOS.WebAPI.Helpers
{
    /// <summary>
    /// HTTP istek helper sınıfı
    /// </summary>
    public static class HttpClientHelper
    {
        /// <summary>
        /// Form data ile POST isteği gönderir
        /// </summary>
        public static async Task<string> PostFormDataAsync(
            IHttpClientFactory httpClientFactory,
            string url,
            Dictionary<string, string> formData,
            CancellationToken cancellationToken = default)
        {
            if (httpClientFactory == null)
                throw new ArgumentNullException(nameof(httpClientFactory));

            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentException("URL boş olamaz", nameof(url));

            if (formData == null || formData.Count == 0)
                throw new ArgumentException("Form data boş olamaz", nameof(formData));

            try
            {
                var httpClient = httpClientFactory.CreateClient();
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
        public static async Task<string> PostXmlAsync(
            IHttpClientFactory httpClientFactory,
            string url,
            string xmlContent,
            CancellationToken cancellationToken = default)
        {
            if (httpClientFactory == null)
                throw new ArgumentNullException(nameof(httpClientFactory));

            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentException("URL boş olamaz", nameof(url));

            if (string.IsNullOrWhiteSpace(xmlContent))
                throw new ArgumentException("XML içeriği boş olamaz", nameof(xmlContent));

            try
            {
                var httpClient = httpClientFactory.CreateClient();

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

