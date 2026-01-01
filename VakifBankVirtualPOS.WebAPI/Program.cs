using Carter;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using VakifBankVirtualPOS.WebAPI.Extensions;
using VakifBankVirtualPOS.WebAPI.Middlewares;

if (args.Length > 0 && args[0] == "encrypt-config")
{
    Console.WriteLine("🔐 Şifreleme modu aktif...\n");
    VakifBankVirtualPOS.WebAPI.Tools.EncryptConfiguration.Run(args);
    return;
}

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOptionsExtensions();
builder.Services.AddWebApiServices(builder.Configuration, builder.Host);
var app = builder.Build();

app.UseHttpsRedirection();

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
.AllowAnonymous()
.WithDisplayName("Complete Health Check")
.WithDescription("Tüm sistemlerin sağlık durumu");

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
    ResultStatusCodes =
    {
        [HealthStatus.Healthy] = StatusCodes.Status200OK,
        [HealthStatus.Degraded] = StatusCodes.Status200OK,
        [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
    }
})
.AllowAnonymous()
.WithDisplayName("Readiness Check")
.WithDescription("Uygulama talep almaya hazır mı?");

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
    ResultStatusCodes =
    {
        [HealthStatus.Healthy] = StatusCodes.Status200OK
    }
})
.AllowAnonymous()
.WithDisplayName("Liveness Check")
.WithDescription("Uygulama çalışıyor mu?");

// Tag bazlı filtreler
app.MapHealthChecks("/health/db", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("db"),
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
})
.AllowAnonymous()
.WithDisplayName("Database Health")
.WithDescription("Veritabanı sağlık kontrolü");

app.MapHealthChecks("/health/external", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("external"),
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
})
.AllowAnonymous()
.WithDisplayName("External Services Health")
.WithDescription("Dış servisler sağlık kontrolü");

app.MapHealthChecks("/health/memory", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("memory"),
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
})
.AllowAnonymous()
.WithDisplayName("Memory Health")
.WithDescription("Bellek kullanımı kontrolü");

// 🎨 Health Checks UI Dashboard
app.MapHealthChecksUI(setup =>
{
    setup.UIPath = "/health-ui";
    setup.ApiPath = "/health-ui-api";
    setup.ResourcesPath = "/health-ui-resources";
    setup.WebhookPath = "/health-ui-webhooks";
    setup.UseRelativeApiPath = false;
    setup.UseRelativeResourcesPath = false;
    setup.UseRelativeWebhookPath = false;
})
.AllowAnonymous();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "EgesehirVakifBankVirtualPOS.WebAPI v1");
    options.RoutePrefix = "swagger";
});

app.UseSession();
app.UseMiddleware<ApiKeyMiddleware>();
app.UseRateLimiter();
app.UseMiddleware<GlobalExceptionMiddleware>();

app.MapCarter();
app.Run();