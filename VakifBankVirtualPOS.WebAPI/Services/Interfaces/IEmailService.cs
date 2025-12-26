namespace VakifBankVirtualPOS.WebAPI.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendPaymentSuccessMailAsync(string orderId, CancellationToken cancellationToken);

        Task SendPaymentFailedMailAsync(string orderId, CancellationToken cancellationToken);
    }
}