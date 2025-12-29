using Carter;
using Microsoft.EntityFrameworkCore;
using VakifBankVirtualPOS.WebAPI.Data.Context;
using VakifBankVirtualPOS.WebAPI.Extensions;
using VakifBankVirtualPOS.WebAPI.Middlewares;

var builder = WebApplication.CreateBuilder(args);

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