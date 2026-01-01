using Microsoft.Extensions.Diagnostics.HealthChecks;
using VakifBankVirtualPOS.WebAPI.Services.Interfaces;

namespace VakifBankVirtualPOS.WebAPI.HealthChecks
{
    public class HybsApiHealthCheck : IHealthCheck
    {
        private readonly IHybsService _hybsService;
        private readonly ILogger<HybsApiHealthCheck> _logger;

        public HybsApiHealthCheck(
            IHybsService hybsService,
            ILogger<HybsApiHealthCheck> logger)
        {
            _hybsService = hybsService;
            _logger = logger;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var startTime = DateTime.Now;
                var token = await _hybsService.GetHybsTokenAsync();
                var duration = DateTime.Now - startTime;

                if (string.IsNullOrEmpty(token))
                {
                    return HealthCheckResult.Unhealthy(
                        "HYBS API token alınamadı",
                        data: new Dictionary<string, object>
                        {
                            { "response_time_ms", duration.TotalMilliseconds },
                            { "timestamp", DateTime.Now }
                        });
                }

                if (duration.TotalSeconds > 5)
                {
                    return HealthCheckResult.Degraded(
                        "HYBS API yavaş yanıt veriyor",
                        data: new Dictionary<string, object>
                        {
                            { "response_time_ms", duration.TotalMilliseconds },
                            { "threshold_ms", 5000 },
                            { "timestamp", DateTime.Now }
                        });
                }

                return HealthCheckResult.Healthy(
                    "HYBS API çalışıyor",
                    data: new Dictionary<string, object>
                    {
                        { "response_time_ms", duration.TotalMilliseconds },
                        { "timestamp", DateTime.Now }
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "HYBS API health check başarısız");

                return HealthCheckResult.Unhealthy(
                    "HYBS API erişilemez",
                    exception: ex,
                    data: new Dictionary<string, object>
                    {
                        { "timestamp", DateTime.Now },
                        { "error", ex.Message }
                    });
            }
        }
    }
}