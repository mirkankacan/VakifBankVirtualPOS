using Microsoft.Extensions.Diagnostics.HealthChecks;
using VakifBankVirtualPOS.WebUI.Options;

namespace VakifBankVirtualPOS.WebUI.HealthChecks
{
    public class BackendApiHealthCheck : IHealthCheck
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ApiOptions _apiOptions;
        private readonly ILogger<BackendApiHealthCheck> _logger;

        public BackendApiHealthCheck(
            IHttpClientFactory httpClientFactory,
            ApiOptions apiOptions,
            ILogger<BackendApiHealthCheck> logger)
        {
            _httpClientFactory = httpClientFactory;
            _apiOptions = apiOptions;
            _logger = logger;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var startTime = DateTime.Now;
                var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(10);

                var response = await client.GetAsync(
                    $"{_apiOptions.BaseUrl}/health",
                    cancellationToken);

                var duration = DateTime.Now - startTime;

                if (!response.IsSuccessStatusCode)
                {
                    return HealthCheckResult.Unhealthy(
                        $"Backend API sağlıksız (HTTP {(int)response.StatusCode})",
                        data: new Dictionary<string, object>
                        {
                            { "api_url", _apiOptions.BaseUrl },
                            { "status_code", (int)response.StatusCode },
                            { "response_time_ms", duration.TotalMilliseconds },
                            { "timestamp", DateTime.Now }
                        });
                }

                if (duration.TotalSeconds > 5)
                {
                    return HealthCheckResult.Degraded(
                        "Backend API yavaş yanıt veriyor",
                        data: new Dictionary<string, object>
                        {
                            { "api_url", _apiOptions.BaseUrl },
                            { "status_code", (int)response.StatusCode },
                            { "response_time_ms", duration.TotalMilliseconds },
                            { "threshold_ms", 5000 },
                            { "timestamp", DateTime.Now }
                        });
                }

                return HealthCheckResult.Healthy(
                    "Backend API çalışıyor",
                    data: new Dictionary<string, object>
                    {
                        { "api_url", _apiOptions.BaseUrl },
                        { "status_code", (int)response.StatusCode },
                        { "response_time_ms", duration.TotalMilliseconds },
                        { "timestamp", DateTime.Now }
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Backend API health check başarısız");

                return HealthCheckResult.Unhealthy(
                    "Backend API erişilemez",
                    exception: ex,
                    data: new Dictionary<string, object>
                    {
                        { "api_url", _apiOptions.BaseUrl },
                        { "timestamp", DateTime.Now },
                        { "error", ex.Message }
                    });
            }
        }
    }
}