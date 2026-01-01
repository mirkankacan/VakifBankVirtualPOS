using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using VakifBankVirtualPOS.WebAPI.Data.Context;

namespace VakifBankVirtualPOS.WebAPI.HealthChecks
{
    public class DatabaseHealthCheck : IHealthCheck
    {
        private readonly AppDbContext _context;
        private readonly ILogger<DatabaseHealthCheck> _logger;

        public DatabaseHealthCheck(
            AppDbContext context,
            ILogger<DatabaseHealthCheck> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var startTime = DateTime.Now;

                var canConnect = await _context.Database.CanConnectAsync(cancellationToken);

                if (!canConnect)
                {
                    return HealthCheckResult.Unhealthy(
                        "Veritabanına bağlanılamadı",
                        data: new Dictionary<string, object>
                        {
                            { "timestamp", DateTime.Now }
                        });
                }

                var tableCount = await _context.IDT_API_KEY.CountAsync(cancellationToken);

                var duration = DateTime.Now - startTime;

                if (duration.TotalSeconds > 3)
                {
                    return HealthCheckResult.Degraded(
                        "Veritabanı yavaş yanıt veriyor",
                        data: new Dictionary<string, object>
                        {
                            { "response_time_ms", duration.TotalMilliseconds },
                            { "threshold_ms", 3000 },
                            { "api_key_count", tableCount },
                            { "timestamp", DateTime.Now }
                        });
                }

                return HealthCheckResult.Healthy(
                    "Veritabanı çalışıyor",
                    data: new Dictionary<string, object>
                    {
                        { "response_time_ms", duration.TotalMilliseconds },
                        { "api_key_count", tableCount },
                        { "timestamp", DateTime.Now }
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database health check başarısız");

                return HealthCheckResult.Unhealthy(
                    "Veritabanı hatası",
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