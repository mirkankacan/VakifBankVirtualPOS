using Carter;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
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
builder.Services.AddHealthCheckServices(builder.Configuration);

var app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles();

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

app.MapCarter();

app.Run();