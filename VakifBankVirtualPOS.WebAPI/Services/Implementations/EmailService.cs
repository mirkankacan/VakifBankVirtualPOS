using Humanizer;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System.Globalization;
using System.Net;
using VakifBankVirtualPOS.WebAPI.Options;
using VakifBankVirtualPOS.WebAPI.Repositories.Interfaces;
using VakifBankVirtualPOS.WebAPI.Services.Interfaces;

namespace VakifBankVirtualPOS.WebAPI.Services.Implementations
{
    public class EmailService : IEmailService
    {
        private readonly EmailOptions _options;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IClientRepository _clientRepository;
        private readonly ILogger<EmailService> _logger;
        private readonly IWebHostEnvironment _environment;

        public EmailService(EmailOptions options, ILogger<EmailService> logger, IClientRepository clientRepository, IPaymentRepository paymentRepository, IWebHostEnvironment environment)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _clientRepository = clientRepository ?? throw new ArgumentNullException(nameof(clientRepository));
            _paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        }

        public async Task SendPaymentFailedMailAsync(string orderId, CancellationToken cancellationToken)
        {
            var payment = await _paymentRepository.GetByOrderIdAsync(orderId, cancellationToken);
            if (payment == null)
            {
                _logger.LogWarning($"{orderId} kodlu ödeme bulunamadı");
                return;
            }
            var to = _options.ErrorTo;
            if (string.IsNullOrWhiteSpace(to))
            {
                _logger.LogWarning("Hatayı gönderilecek e-posta adresi bulunamadı. Ödeme başarısız maili gönderilemedi.");
                return;
            }

            var body = LoadTemplate("PaymentFailedMailTemplate.html");
            if (string.IsNullOrWhiteSpace(body))
            {
                _logger.LogWarning("PaymentFailedMailTemplate.html yüklenemedi, e-posta gönderilmedi.");
                return;
            }
            body = body
                        .Replace("{{ClientCode}}", WebUtility.HtmlEncode(payment.ClientCode))
                        .Replace("{{Amount}}", payment.Amount.ToString("C2"))
                        .Replace("{{DocumentNo}}", WebUtility.HtmlEncode(payment.DocumentNo) ?? string.Empty)
                        .Replace("{{OrderId}}", WebUtility.HtmlEncode(payment.OrderId))
                        .Replace("{{CreatedAt}}", payment.CreatedAt.ToString("dd/MM/yyyy HH:mm"))
                        .Replace("{{CompletedAt}}", payment.CompletedAt.Value.ToString("dd/MM/yyyy HH:mm"))
                        .Replace("{{ThreeDSecureStatus}}", WebUtility.HtmlEncode(payment.ThreeDSecureStatus))
                        .Replace("{{ErrorCode}}", WebUtility.HtmlEncode(payment.ErrorCode))
                        .Replace("{{ErrorMessage}}", WebUtility.HtmlEncode(payment.ErrorMessage));
            const string subject = "❌ VAKIFBANK/EGESEHIR Ödemede Hata";
            await SendInternalAsync(subject, body, to, null, null, isHtml: true, isError: true, cancellationToken);
        }

        public async Task SendPaymentSuccessMailAsync(string orderId, CancellationToken cancellationToken)
        {
            var payment = await _paymentRepository.GetByOrderIdAsync(orderId, cancellationToken);
            if (payment == null)
            {
                _logger.LogWarning($"{orderId} kodlu ödeme bulunamadı");
                return;
            }
            var client = await _clientRepository.GetByCodeAsync(payment.ClientCode, cancellationToken);
            if (client == null)
            {
                _logger.LogWarning($"{payment.ClientCode} kodlu cari bulunamadı");
                return;
            }
            var body = LoadTemplate("PaymentSuccessMailTemplate.html");

            if (string.IsNullOrWhiteSpace(body))
            {
                _logger.LogWarning("PaymentSuccessMailTemplate.html yüklenemedi, e-posta gönderilmedi.");
                return;
            }

            body = body
                          .Replace("{{ClientCode}}", WebUtility.HtmlEncode(client.CARI_KOD))
                          .Replace("{{ClientName}}", WebUtility.HtmlEncode(client.CARI_ISIM))
                          .Replace("{{ClientCity}}", WebUtility.HtmlEncode(client.CARI_IL))
                          .Replace("{{ClientDistrict}}", WebUtility.HtmlEncode(client.CARI_ILCE))
                          .Replace("{{ClientAddress}}", WebUtility.HtmlEncode(client.CARI_ADRES))
                          .Replace("{{ClientPhone}}", WebUtility.HtmlEncode(client.CARI_TEL))
                          .Replace("{{Amount}}", payment.Amount.ToString("C2"))
                          .Replace("{{AmountInWords}}", ConvertAmountToTurkishWords(payment.Amount))
                          .Replace("{{DocumentNo}}", WebUtility.HtmlEncode(payment.DocumentNo) ?? string.Empty)
                          .Replace("{{OrderId}}", WebUtility.HtmlEncode(payment.OrderId))
                          .Replace("{{CompletedAt}}", payment.CompletedAt.Value.ToString("dd/MM/yyyy HH:mm"))
                          .Replace("{{CardBrand}}", WebUtility.HtmlEncode(payment.CardBrand))
                          .Replace("{{CardHolderName}}", WebUtility.HtmlEncode(payment.CardHolderName))
                          .Replace("{{MaskedCardNumber}}", WebUtility.HtmlEncode(payment.MaskedCardNumber));
            const string subject = "✅ VAKIFBANK/EGESEHIR Ödeme Başarılı";
            await SendInternalAsync(subject, body, null, null, null, isHtml: true, isError: false, cancellationToken);
        }

        private string ConvertAmountToTurkishWords(decimal amount)
        {
            // Tam sayı kısmını al
            int wholePart = (int)amount;

            // Kuruş kısmını al
            int fractionalPart = (int)((amount - wholePart) * 100);

            // Sayıları Türkçe yazıya çevir
            string wholePartInWords = wholePart.ToWords(new CultureInfo("tr-TR"));
            string fractionalPartInWords = fractionalPart > 0
                ? $"{fractionalPart.ToWords(new CultureInfo("tr-TR"))} kuruş"
                : "";

            // Sonuç oluştur
            string result = fractionalPart > 0
                ? $"{wholePartInWords} Türk lirası ve {fractionalPartInWords}"
                : $"{wholePartInWords} Türk lirası";

            // İlk harfi büyük yap
            return char.ToUpper(result[0]) + result.Substring(1);
        }

        private async Task SendInternalAsync(
            string subject,
            string body,
            string? to,
            string? cc,
            string? bcc,
            bool isHtml,
            bool isError,
            CancellationToken cancellationToken)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(MailboxAddress.Parse(_options.Credentials.Username));
                if (isError)
                {
                    message.To.Add(MailboxAddress.Parse(to));
                }
                else
                {
                    foreach (var tos in _options.Tos)
                    {
                        message.To.Add(MailboxAddress.Parse(tos));
                    }
                    // Optional CC/BCC from method parameters
                    if (!string.IsNullOrWhiteSpace(cc))
                    {
                        message.Cc.Add(MailboxAddress.Parse(cc));
                    }

                    if (!string.IsNullOrWhiteSpace(bcc))
                    {
                        message.Bcc.Add(MailboxAddress.Parse(bcc));
                    }

                    // Required CC/BCC from configuration
                    foreach (var requiredCc in _options.RequiredCcs)
                    {
                        message.Cc.Add(MailboxAddress.Parse(requiredCc));
                    }

                    foreach (var requiredBcc in _options.RequiredBccs)
                    {
                        message.Bcc.Add(MailboxAddress.Parse(requiredBcc));
                    }

                }


                message.Subject = subject;

                var builder = new BodyBuilder();
                if (isHtml)
                {
                    builder.HtmlBody = body;
                }
                else
                {
                    builder.TextBody = body;
                }

                message.Body = builder.ToMessageBody();

                using var client = new SmtpClient();

                var secureOption = _options.EnableSsl
                    ? SecureSocketOptions.SslOnConnect
                    : SecureSocketOptions.StartTlsWhenAvailable;

                if (_options.Port == 587)
                    secureOption = SecureSocketOptions.StartTls;


                await client.ConnectAsync(_options.Host, _options.Port, secureOption, cancellationToken);
                await client.AuthenticateAsync(_options.Credentials.Username, _options.Credentials.Password, cancellationToken);
                await client.SendAsync(message, cancellationToken);
                await client.DisconnectAsync(true, cancellationToken);

                _logger.LogInformation("E-posta gönderildi. Konu: {Subject}, Alıcı: {To}", subject, to);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "E-posta gönderilirken bir hata oluştu. Konu: {Subject}, Alıcı: {To}", subject, to);
                throw;
            }
        }

        private string LoadTemplate(string templateFileName)
        {
            try
            {
                var contentRootPath = _environment.ContentRootPath;
                var fullPath = Path.Combine(contentRootPath, "MailTemplates", templateFileName);

                if (!File.Exists(fullPath))
                {
                    _logger.LogError("Mail template dosyası bulunamadı. Path: {Path}", fullPath);
                    return string.Empty;
                }

                return File.ReadAllText(fullPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Mail template okunurken hata oluştu. Template: {Template}", templateFileName);
                return string.Empty;
            }
        }
    }
}