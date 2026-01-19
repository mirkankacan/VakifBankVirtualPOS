using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using VakifBankVirtualPOS.WebAPI.HealthChecks;
using VakifBankVirtualPOS.WebAPI.Options;

namespace VakifBankVirtualPOS.WebAPI.Extensions
{
    public static class HealthCheckExtension
    {
        public static IServiceCollection AddHealthCheckServices(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("SqlConnection");
            var uiOptions = configuration.GetSection("UiOptions").Get<UiOptions>()!;
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            //  Health Checks
            services.AddHealthChecks()
                //  SQL Server Bağlantısı
                .AddSqlServer(
                    connectionString: connectionString!,
                    healthQuery: "SELECT 1;",
                    name: "sql-server",
                    failureStatus: HealthStatus.Unhealthy,
                    tags: new[] { "database" })

                //  Database İşlemleri
                .AddCheck<DatabaseHealthCheck>(
                    "database-operations",
                    failureStatus: HealthStatus.Degraded,
                    tags: new[] { "database" })

                //  VakıfBank Enrollment API
                .AddUrlGroup(
                    uri: new Uri(configuration["VakifBankOptions:EnrollmentUrl"]!),
                    name: "vakifbank-enrollment",
                    failureStatus: HealthStatus.Degraded,
                    timeout: TimeSpan.FromSeconds(10),
                    tags: new[] { "external" })

                //  VakıfBank VPOS API
                .AddUrlGroup(
                    uri: new Uri(configuration["VakifBankOptions:VposUrl"]!),
                    name: "vakifbank-vpos",
                    failureStatus: HealthStatus.Degraded,
                    timeout: TimeSpan.FromSeconds(10),
                    tags: new[] { "external" })

                //  Email SMTP
                .AddCheck<EmailServiceHealthCheck>(
                    "email-smtp",
                    failureStatus: HealthStatus.Degraded,
                    tags: new[] { "external" });

            //  Health Checks UI
            services
                .AddHealthChecksUI(setup =>
                {
                    setup.SetEvaluationTimeInSeconds(30);
                    setup.MaximumHistoryEntriesPerEndpoint(100);

                    // WebAPI kendi endpoint'i
                    setup.AddHealthCheckEndpoint(
                        "Egeşehir VakıfBank Virtual POS API",
                        "/health");

                    // WebUI endpoint'i (Development dışında)
                    if (environment != "Development")
                    {
                        setup.AddHealthCheckEndpoint(
                            "Egeşehir VakıfBank Virtual POS Web UI",
                            uiOptions.BaseUrl + "/health");
                    }
                    else
                    {
                        setup.AddHealthCheckEndpoint(
                          "Egeşehir VakıfBank Virtual POS Web UI",
                         "https://localhost:8484/health");
                    }
                })
                .AddInMemoryStorage();

            return services;
        }

        public static IEndpointRouteBuilder MapHealthCheckServices(this IEndpointRouteBuilder app)
        {
            // Health Check Endpoint
            app.MapHealthChecks("/health", new HealthCheckOptions
            {
                Predicate = _ => true,
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
                ResultStatusCodes =
                {
                    [HealthStatus.Healthy] = StatusCodes.Status200OK,
                    [HealthStatus.Degraded] = StatusCodes.Status200OK,
                    [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
                }
            })
            .AllowAnonymous();

            //  Health UI Dashboard
            app.MapHealthChecksUI(setup =>
            {
                setup.UIPath = "/health-ui";
                setup.ApiPath = "/health-ui-api";
                setup.ResourcesPath = "/health-ui-resources";
                setup.WebhookPath = "/health-ui-webhook";
            })
            .AllowAnonymous();
            return app;
        }
    }
}