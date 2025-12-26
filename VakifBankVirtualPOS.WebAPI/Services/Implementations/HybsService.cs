using VakifBankVirtualPOS.WebAPI.Options;
using VakifBankVirtualPOS.WebAPI.Services.Interfaces;

namespace VakifBankVirtualPOS.WebAPI.Services.Implementations
{
    public class HybsService : IHybsService
    {
        private readonly HybsOptions _options;

        public HybsService(HybsOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }
    }
}