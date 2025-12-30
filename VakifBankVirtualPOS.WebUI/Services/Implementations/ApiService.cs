using System.Net;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using VakifBankVirtualPOS.WebUI.Common;
using VakifBankVirtualPOS.WebUI.Services.Interfaces;

namespace VakifBankVirtualPOS.WebUI.Services.Implementations
{
    public class ApiService : IApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ApiService> _logger;

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = null,
            PropertyNameCaseInsensitive = true,
            WriteIndented = false,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        public ApiService(HttpClient httpClient, ILogger<ApiService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<ApiResponse<T>> GetAsync<T>(string endpoint, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("GET isteği gönderiliyor: {Endpoint}", endpoint);

                var response = await _httpClient.GetAsync(endpoint, cancellationToken);
                return await HandleResponse<T>(response, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GET isteği başarısız: {Endpoint}", endpoint);
                return ApiResponse<T>.Failure("İstek sırasında bir hata oluştu", HttpStatusCode.InternalServerError);
            }
        }

        public async Task<ApiResponse<T>> PostAsync<T>(string endpoint, object data, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("POST isteği gönderiliyor: {Endpoint}", endpoint);

                var json = JsonSerializer.Serialize(data, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(endpoint, content, cancellationToken);
                return await HandleResponse<T>(response, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "POST isteği başarısız: {Endpoint}", endpoint);
                return ApiResponse<T>.Failure("İstek sırasında bir hata oluştu", HttpStatusCode.InternalServerError);
            }
        }

        public async Task<ApiResponse<T>> PutAsync<T>(string endpoint, object data, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("PUT isteği gönderiliyor: {Endpoint}", endpoint);

                var json = JsonSerializer.Serialize(data, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync(endpoint, content, cancellationToken);
                return await HandleResponse<T>(response, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PUT isteği başarısız: {Endpoint}", endpoint);
                return ApiResponse<T>.Failure("İstek sırasında bir hata oluştu", HttpStatusCode.InternalServerError);
            }
        }

        public async Task<ApiResponse<T>> DeleteAsync<T>(string endpoint, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("DELETE isteği gönderiliyor: {Endpoint}", endpoint);

                var response = await _httpClient.DeleteAsync(endpoint, cancellationToken);
                return await HandleResponse<T>(response, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DELETE isteği başarısız: {Endpoint}", endpoint);
                return ApiResponse<T>.Failure("İstek sırasında bir hata oluştu", HttpStatusCode.InternalServerError);
            }
        }

        // Response döndürmeyen metodlar (NoContent gibi)
        public async Task<ApiResponse> PostAsync(string endpoint, object data, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("POST isteği gönderiliyor: {Endpoint}", endpoint);

                var json = JsonSerializer.Serialize(data, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(endpoint, content, cancellationToken);
                return await HandleResponse(response, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "POST isteği başarısız: {Endpoint}", endpoint);
                return ApiResponse.Failure("İstek sırasında bir hata oluştu", HttpStatusCode.InternalServerError);
            }
        }

        public async Task<ApiResponse> PutAsync(string endpoint, object data, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("PUT isteği gönderiliyor: {Endpoint}", endpoint);

                var json = JsonSerializer.Serialize(data, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync(endpoint, content, cancellationToken);
                return await HandleResponse(response, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PUT isteği başarısız: {Endpoint}", endpoint);
                return ApiResponse.Failure("İstek sırasında bir hata oluştu", HttpStatusCode.InternalServerError);
            }
        }

        public async Task<ApiResponse> DeleteAsync(string endpoint, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("DELETE isteği gönderiliyor: {Endpoint}", endpoint);

                var response = await _httpClient.DeleteAsync(endpoint, cancellationToken);
                return await HandleResponse(response, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DELETE isteği başarısız: {Endpoint}", endpoint);
                return ApiResponse.Failure("İstek sırasında bir hata oluştu", HttpStatusCode.InternalServerError);
            }
        }

        private async Task<ApiResponse<T>> HandleResponse<T>(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            var statusCode = response.StatusCode;

            if (response.IsSuccessStatusCode)
            {
                if (statusCode == HttpStatusCode.NoContent)
                {
                    return ApiResponse<T>.Success(default, statusCode);
                }

                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                var data = JsonSerializer.Deserialize<T>(content, _jsonOptions);
                return ApiResponse<T>.Success(data, statusCode);
            }

            // Hata durumunda ProblemDetails parse et
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            string errorTitle = null;
            string errorDetail = null;

            try
            {
                var problemDetails = JsonSerializer.Deserialize<ProblemDetailsResponse>(errorContent, _jsonOptions);

                if (problemDetails != null)
                {
                    errorTitle = problemDetails.Title;
                    errorDetail = problemDetails.Detail;
                }

                // Eğer title veya detail yoksa, ham içeriği kullan
                if (string.IsNullOrEmpty(errorTitle) && string.IsNullOrEmpty(errorDetail))
                {
                    errorDetail = !string.IsNullOrEmpty(errorContent)
                        ? errorContent
                        : $"Bir hata oluştu. (HTTP {(int)statusCode})";
                }
            }
            catch
            {
                // JSON parse edilemezse ham içeriği detail olarak kullan
                errorDetail = !string.IsNullOrEmpty(errorContent)
                    ? errorContent
                    : $"Bir hata oluştu. (HTTP {(int)statusCode})";
            }

            _logger.LogWarning("API hatası: {StatusCode} - Title: {Title}, Detail: {Detail}", statusCode, errorTitle, errorDetail);
            
            if (!string.IsNullOrEmpty(errorTitle) || !string.IsNullOrEmpty(errorDetail))
            {
                return ApiResponse<T>.Failure(errorTitle ?? "Hata", errorDetail ?? $"Bir hata oluştu. (HTTP {(int)statusCode})", statusCode);
            }
            
            return ApiResponse<T>.Failure($"Bir hata oluştu. (HTTP {(int)statusCode})", statusCode);
        }

        private async Task<ApiResponse> HandleResponse(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            var statusCode = response.StatusCode;

            if (response.IsSuccessStatusCode)
            {
                return ApiResponse.Success(statusCode);
            }

            // Hata durumunda ProblemDetails parse et
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            string errorTitle = null;
            string errorDetail = null;

            try
            {
                var problemDetails = JsonSerializer.Deserialize<ProblemDetailsResponse>(errorContent, _jsonOptions);

                if (problemDetails != null)
                {
                    errorTitle = problemDetails.Title;
                    errorDetail = problemDetails.Detail;
                }

                // Eğer title veya detail yoksa, ham içeriği kullan
                if (string.IsNullOrEmpty(errorTitle) && string.IsNullOrEmpty(errorDetail))
                {
                    errorDetail = !string.IsNullOrEmpty(errorContent)
                        ? errorContent
                        : $"Bir hata oluştu. (HTTP {(int)statusCode})";
                }
            }
            catch
            {
                // JSON parse edilemezse ham içeriği detail olarak kullan
                errorDetail = !string.IsNullOrEmpty(errorContent)
                    ? errorContent
                    : $"Bir hata oluştu. (HTTP {(int)statusCode})";
            }

            _logger.LogWarning("API hatası: {StatusCode} - Title: {Title}, Detail: {Detail}", statusCode, errorTitle, errorDetail);
            
            if (!string.IsNullOrEmpty(errorTitle) || !string.IsNullOrEmpty(errorDetail))
            {
                return ApiResponse.Failure(errorTitle ?? "Hata", errorDetail ?? $"Bir hata oluştu. (HTTP {(int)statusCode})", statusCode);
            }
            
            return ApiResponse.Failure($"Bir hata oluştu. (HTTP {(int)statusCode})", statusCode);
        }
    }
}