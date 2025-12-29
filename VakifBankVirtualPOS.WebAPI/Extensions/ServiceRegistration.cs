using Carter;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer;
using System.Collections.ObjectModel;
using System.Data;
using System.Net;
using System.Threading.RateLimiting;
using VakifBankPayment.Services.Implementations;
using VakifBankVirtualPOS.WebAPI.Data.Context;
using VakifBankVirtualPOS.WebAPI.Helpers;
using VakifBankVirtualPOS.WebAPI.Middlewares;
using VakifBankVirtualPOS.WebAPI.Options;
using VakifBankVirtualPOS.WebAPI.Repositories.Implementations;
using VakifBankVirtualPOS.WebAPI.Repositories.Interfaces;
using VakifBankVirtualPOS.WebAPI.Services.Implementations;
using VakifBankVirtualPOS.WebAPI.Services.Interfaces;

namespace VakifBankVirtualPOS.WebAPI.Extensions
{
    public static class ServiceRegistration
    {
        public static IServiceCollection AddWebApiServices(this IServiceCollection services, IConfiguration configuration, IHostBuilder host)
        {
            var connectionString = configuration.GetConnectionString("SqlConnection");
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var emailOptions = configuration.GetSection("EmailOptions").Get<EmailOptions>()!;
            services.AddScoped<GlobalExceptionMiddleware>();

            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(connectionString));

            services.AddHttpClient();
            services.AddHttpContextAccessor();
            services.AddDistributedMemoryCache();
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(5);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.SameSite = SameSiteMode.Strict;
                options.Cookie.Name = ".EgesehirVakifBankVirtualPOS.WebAPISession";
            });
            services.AddRateLimiter(options =>
            {
                // Global rate limit - IP bazlı
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                {
                    var httpContextAccessor = context.RequestServices.GetRequiredService<IHttpContextAccessor>();
                    var clientIp = IpHelper.GetClientIp(httpContextAccessor);

                    return RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: clientIp,
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 100, // 100 istek
                            Window = TimeSpan.FromMinutes(1), // 1 dakikada
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = 0
                        });
                });

                // Ödeme endpoint'leri için daha kısıtlayıcı limit
                options.AddPolicy("payment", context =>
                {
                    var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                    return RateLimitPartition.GetSlidingWindowLimiter(
                        partitionKey: clientIp,
                        factory: _ => new SlidingWindowRateLimiterOptions
                        {
                            PermitLimit = 5, // 5 ödeme isteği
                            Window = TimeSpan.FromMinutes(1), // 1 dakikada
                            SegmentsPerWindow = 2,
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = 0
                        });
                });

                // Client kontrol endpoint'i için
                options.AddPolicy("client", context =>
                {
                    var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                    return RateLimitPartition.GetTokenBucketLimiter(
                        partitionKey: clientIp,
                        factory: _ => new TokenBucketRateLimiterOptions
                        {
                            TokenLimit = 20,
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = 0,
                            ReplenishmentPeriod = TimeSpan.FromMinutes(1),
                            TokensPerPeriod = 10,
                            AutoReplenishment = true
                        });
                });

                options.OnRejected = async (context, cancellationToken) =>
                {
                    context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

                    await context.HttpContext.Response.WriteAsJsonAsync(new ProblemDetails
                    {
                        Status = StatusCodes.Status429TooManyRequests,
                        Title = "Çok Fazla İstek",
                        Detail = "Çok fazla istek gönderdiniz. Lütfen bir süre bekleyip tekrar deneyin.",
                        Instance = context.HttpContext.Request.Path
                    }, cancellationToken);
                };
            });
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Application", "EgesehirVakifBankVirtualPOS.WebAPI")
                .Enrich.WithProperty("Environment", environment)
                .Enrich.WithClientIp()
                .Enrich.WithMachineName()
                .Enrich.WithThreadId()
                .WriteTo.Logger(lc =>
                {
                    if (environment == "Development")
                    {
                        lc.WriteTo.Console(
                            restrictedToMinimumLevel: LogEventLevel.Debug,
                            outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}");
                    }
                })
                .WriteTo.File("logs/api-.log",
                    restrictedToMinimumLevel: LogEventLevel.Information,
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 30,
                    fileSizeLimitBytes: 10_000_000,
                    rollOnFileSizeLimit: true,
                    shared: true,
                    outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
                .WriteTo.MSSqlServer(
                    connectionString: connectionString,
                    sinkOptions: new MSSqlServerSinkOptions
                    {
                        TableName = "IDT_EGESEHIR_ETAHSILAT_LOGS",
                        SchemaName = "dbo",
                        AutoCreateSqlTable = true,
                        BatchPostingLimit = 500,
                        BatchPeriod = TimeSpan.FromSeconds(15)
                    },
                    restrictedToMinimumLevel: LogEventLevel.Information,
                    columnOptions: GetColumnOptions())
                .WriteTo.Email(
                    from: emailOptions.Credentials.Username,
                    to: emailOptions.ErrorTo,
                    host: emailOptions.Host,
                    port: emailOptions.Port,
                    connectionSecurity: emailOptions.EnableSsl
                        ? MailKit.Security.SecureSocketOptions.SslOnConnect
                        : MailKit.Security.SecureSocketOptions.StartTlsWhenAvailable,
                    credentials: new NetworkCredential(
                        emailOptions.Credentials.Username,
                        emailOptions.Credentials.Password
                    ),
                    subject: "🚨 EgesehirVakifBankVirtualPOS.WebAPI - {Level} - {Timestamp:dd-MM-yyyy HH:mm}",
                    restrictedToMinimumLevel: LogEventLevel.Error
                )
                .CreateLogger();

            host.UseSerilog();

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "EgesehirVakifBankVirtualPOS.WebAPI",
                    Version = "v1",
                    Description = "VakıfBank Sanal POS Web API"
                });

                //options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                //{
                //    Name = "Authorization",
                //    Type = SecuritySchemeType.Http,
                //    Scheme = "Bearer",
                //    BearerFormat = "JWT",
                //    In = ParameterLocation.Header,
                //    Description = "JWT token giriniz. Sadece token yazın, 'Bearer' eklemeyin.\n\nÖrnek: eyJhbGciOiJIUzI1NiIs..."
                //});

                //options.AddSecurityRequirement(new OpenApiSecurityRequirement
                //{
                //    {
                //        new OpenApiSecurityScheme
                //        {
                //            Reference = new OpenApiReference
                //            {
                //                Type = ReferenceType.SecurityScheme,
                //                Id = "Bearer"
                //            }
                //        },
                //        Array.Empty<string>()
                //    }
                //});
            });
            services.AddMapster();
            var config = TypeAdapterConfig.GlobalSettings;
            config.Scan(typeof(WebAPIAssembly).Assembly);

            services.AddCarter();
            services.AddEndpointsApiExplorer();

            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IPaymentRepository, PaymentRepository>();
            services.AddScoped<IClientRepository, ClientRepository>();
            services.AddScoped<IClientService, ClientService>();
            services.AddScoped<IPaymentService, PaymentService>();
            services.AddScoped<IHybsService, HybsService>();
            return services;
        }

        private static ColumnOptions GetColumnOptions()
        {
            var columnOptions = new ColumnOptions();

            columnOptions.Store.Remove(StandardColumn.MessageTemplate);

            columnOptions.Store.Add(StandardColumn.LogEvent);

            columnOptions.DisableTriggers = true;

            columnOptions.AdditionalColumns = new Collection<SqlColumn>
            {
                new SqlColumn { ColumnName = "ClientCode", DataType = SqlDbType.NVarChar, DataLength = 50, AllowNull = true },
                new SqlColumn { ColumnName = "Action", DataType = SqlDbType.NVarChar, DataLength = 100, AllowNull = true },
                new SqlColumn { ColumnName = "Module", DataType = SqlDbType.NVarChar, DataLength = 100, AllowNull = true },
                new SqlColumn { ColumnName = "ClientIP", DataType = SqlDbType.NVarChar, DataLength = 45, AllowNull = true },
                new SqlColumn { ColumnName = "UserAgent", DataType = SqlDbType.NVarChar, DataLength = 500, AllowNull = true },
                new SqlColumn { ColumnName = "RequestId", DataType = SqlDbType.NVarChar, DataLength = 50, AllowNull = true }
            };

            return columnOptions;
        }
    }
}