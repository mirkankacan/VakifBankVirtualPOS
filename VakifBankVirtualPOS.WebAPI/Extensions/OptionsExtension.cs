using Microsoft.Extensions.Options;
using VakifBankVirtualPOS.WebAPI.Options;

namespace VakifBankVirtualPOS.WebAPI.Extensions
{
    public static class OptionsExtension
    {
        public static IServiceCollection AddOptionsExtensions(this IServiceCollection services)
        {
            services.AddValidatedOptions<VakifBankOptions>();
            services.AddValidatedOptions<EmailOptions>();
            services.AddValidatedOptions<HybsOptions>();

            return services;
        }

        private static IServiceCollection AddValidatedOptions<TOptions>(this IServiceCollection services)
            where TOptions : class
        {
            services.AddOptions<TOptions>()
                .BindConfiguration(typeof(TOptions).Name)
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services.AddSingleton(sp => sp.GetRequiredService<IOptions<TOptions>>().Value);

            return services;
        }
    }
}