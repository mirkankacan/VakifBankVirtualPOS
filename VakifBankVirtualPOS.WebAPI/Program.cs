using Carter;
using Microsoft.EntityFrameworkCore;
using VakifBankVirtualPOS.WebAPI.Configuration;
using VakifBankVirtualPOS.WebAPI.Extensions;
using VakifBankVirtualPOS.WebAPI.Middlewares;

if (args.Length > 0 && args[0] == "encrypt-config")
{
    Console.WriteLine("🔐 Şifreleme modu aktif...\n");
    VakifBankVirtualPOS.WebAPI.Tools.EncryptConfiguration.Run(args);
    return;
}

var builder = WebApplication.CreateBuilder(args);
if (builder.Environment.IsProduction())
{
    var encryptedConfigPath = Path.Combine(
        builder.Environment.ContentRootPath,
        "appsettings.Production.enc"
    );

    try
    {
        if (File.Exists(encryptedConfigPath))
        {
            builder.Configuration.AddEncryptedJsonFile(encryptedConfigPath);
        }
        else
        {
            Console.WriteLine($"⚠️  UYARI: Şifreli config dosyası bulunamadı!");
            Console.WriteLine($"   Yol: {encryptedConfigPath}");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"   Dosya: {encryptedConfigPath}");
        Console.WriteLine($"   Hata: {ex.Message}");
        throw; // Uygulama başlamasın
    }
}

builder.Services.AddOptionsExtensions();
builder.Services.AddWebApiServices(builder.Configuration, builder.Host);
var app = builder.Build();
app.UseHttpsRedirection();

app.UseSession();
app.UseRateLimiter();
app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "VakifBankVirtualPOS.WebAPI v1");
    options.RoutePrefix = "swagger";
});

app.MapCarter();
app.Run();