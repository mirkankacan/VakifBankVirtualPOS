using Carter;
using Microsoft.EntityFrameworkCore;
using VakifBankVirtualPOS.WebAPI.Configuration;
using VakifBankVirtualPOS.WebAPI.Data.Context;
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
        Console.WriteLine($"📂 Şifreli config yükleniyor: {encryptedConfigPath}");

        if (File.Exists(encryptedConfigPath))
        {
            builder.Configuration.AddEncryptedJsonFile(encryptedConfigPath);
            Console.WriteLine("✅ Şifreli config başarıyla yüklendi!");
        }
        else
        {
            Console.WriteLine($"⚠️  UYARI: Şifreli config dosyası bulunamadı!");
            Console.WriteLine($"   Yol: {encryptedConfigPath}");
            Console.WriteLine($"   Normal appsettings.json kullanılacak.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ KRİTİK HATA: Şifreli config yüklenemedi!");
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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "VakifBankVirtualPOS.WebAPI v1");
        options.RoutePrefix = "swagger";
    });
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    context.Database.EnsureCreated();

    context.Database.Migrate();
}

app.MapCarter();
app.Run();