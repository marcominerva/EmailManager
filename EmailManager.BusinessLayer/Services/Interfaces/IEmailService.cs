using EmailManager.Shared.Models;

namespace EmailManager.BusinessLayer.Services.Interfaces;

public interface IEmailService
{
    Task<SendEmailResult> SendAsync(EmailMessage emailMessage, CancellationToken cancellationToken = default);
}
