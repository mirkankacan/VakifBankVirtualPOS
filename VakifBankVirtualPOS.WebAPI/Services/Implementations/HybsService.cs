using VakifBankVirtualPOS.WebAPI.Dtos.HybsDtos;
using VakifBankVirtualPOS.WebAPI.Options;
using VakifBankVirtualPOS.WebAPI.Services.Interfaces;

namespace VakifBankVirtualPOS.WebAPI.Services.Implementations
{
    public class HybsService : IHybsService
    {
        private readonly HybsOptions _options;
        private readonly HttpClient _httpClient;
        private readonly ILogger<HybsService> _logger;

        public HybsService(HybsOptions options, HttpClient httpClient, ILogger<HybsService> logger)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger;
        }

        public async Task<ClientDetailDto?> GetClientDetailByNoAsync(string no, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(no))
                    throw new ArgumentException("Vergi/TC numarası boş olamaz", nameof(no));

                var token = await GetHybsTokenAsync();

                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", token);

                var response = await _httpClient.GetAsync(
                    $"https://hybs.izmir.bel.tr/HYS.WebApi/api/SahaIsletmeciEntegrasyon/FirmaListesi?VergiNo={no}&Tur=A"
                );

                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<ClientDetailDto>();
                _logger.LogInformation("HYBS sisteminden cari detayları başarıyla alındı");

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "HYBS sisteminden cari detayları alınırken hata");
                throw;
            }
        }

        public async Task<string> GetHybsTokenAsync()
        {
            try
            {
                var credentials = new
                {
                    username = _options.Username,
                    password = _options.Password,
                    IsMobile = _options.IsMobile
                };

                var response = await _httpClient.PostAsJsonAsync(
                    "https://hybs.izmir.bel.tr/HYS.WebApi/api/User/CheckUser",
                    credentials
                );

                response.EnsureSuccessStatusCode();

                var tokenResult = await response.Content.ReadFromJsonAsync<TokenResult>() ?? throw new InvalidOperationException("Token alınamadı");
                _logger.LogInformation("HYBS sisteminden token başarıyla alındı");

                return tokenResult.authtoken;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "HYBS sisteminden token alınırken hata");
                throw;
            }
        }
    }
}