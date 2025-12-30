using Microsoft.EntityFrameworkCore;
using VakifBankVirtualPOS.WebAPI.Data.Context;

namespace VakifBankVirtualPOS.WebAPI.Middlewares
{
    public class ApiKeyMiddleware
    {
        private readonly RequestDelegate _next;
        private const string API_KEY_HEADER = "X-API-Key";

        public ApiKeyMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, AppDbContext dbContext)
        {
            // API Key kontrolü
            if (!context.Request.Headers.TryGetValue(API_KEY_HEADER, out var extractedApiKey))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsJsonAsync(new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    title = "Unauthorized",
                    status = 401,
                    detail = "API anahtarı bulunamadı. Lütfen X-API-Key header'ını ekleyin. Sistem yöneticisiyle iletişime geçiniz."
                });
                return;
            }

            var apiKey = await dbContext.IDT_API_KEY
                .FirstOrDefaultAsync(k => k.ApiKey == extractedApiKey.ToString());

            if (apiKey == null)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsJsonAsync(new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    title = "Unauthorized",
                    status = 401,
                    detail = "Geçersiz API Key. Sistem yöneticisiyle iletişime geçiniz."
                });
                return;
            }

            if (!apiKey.IsActive)
            {
                context.Response.StatusCode = 403;
                await context.Response.WriteAsJsonAsync(new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    title = "Forbidden",
                    status = 403,
                    detail = "API Key devre dışı. Sistem yöneticisiyle iletişime geçiniz."
                });
                return;
            }

            if (apiKey.ExpiresAt.HasValue && apiKey.ExpiresAt.Value < DateTime.Now)
            {
                context.Response.StatusCode = 403;
                await context.Response.WriteAsJsonAsync(new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    title = "Forbidden",
                    status = 403,
                    detail = "API Key süresi dolmuş. Sistem yöneticisiyle iletişime geçiniz."
                });
                return;
            }

            // Son kullanım tarihini güncelle
            apiKey.LastUsedAt = DateTime.Now;
            await dbContext.SaveChangesAsync();

            await _next(context);
        }
    }
}