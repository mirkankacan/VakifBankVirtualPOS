using Carter;
using VakifBankVirtualPOS.WebAPI.Extensions;

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

app.UseWebApiServices();
app.MapHealthCheckServices();

app.MapCarter();
app.MapGet("/api/whats-my-ip", (HttpContext context) =>
{
    var remoteIp = context.Connection.RemoteIpAddress?.ToString();
    var xForwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
    var xRealIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();

    return new
    {
        RemoteIpAddress = remoteIp,
        XForwardedFor = xForwardedFor,
        XRealIP = xRealIp
    };
});
app.Run();