using EmailManager.BusinessLayer.Services.Interfaces;
using EmailManager.BusinessLayer.Settings;
using EmailManager.Shared.Models;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace EmailManager.BusinessLayer.Services;

public class SmtpEmailService(IConfiguration configuration) : IEmailService
{
    private readonly SmtpSettings settings = configuration.GetSection("EmailSettings:SmtpSettings").Get<SmtpSettings>() ?? new();

    public async Task<SendEmailResult> SendAsync(EmailMessage emailMessage, CancellationToken cancellationToken = default)
    {
        var email = CreateEmail(emailMessage);

        using var client = new SmtpClient();

        if (settings.IgnoreServerCertificateErrors)
        {
            client.ServerCertificateValidationCallback = (_, _, _, _) => true;
        }

        await client.ConnectAsync(settings.Host, settings.Port, settings.UseSsl, cancellationToken);

        if (!string.IsNullOrWhiteSpace(settings.UserName) && !string.IsNullOrWhiteSpace(settings.Password))
        {
            await client.AuthenticateAsync(settings.UserName, settings.Password, cancellationToken);
        }

        var response = await client.SendAsync(email, cancellationToken);

        await client.DisconnectAsync(true, cancellationToken);

        return new SendEmailResult(StatusCodes.Status200OK, email.MessageId);
    }

    private static MimeMessage CreateEmail(EmailMessage message)
    {
        var email = new MimeMessage();

        email.From.Add(new MailboxAddress(message.SenderName ?? message.SenderEmail, message.SenderEmail));
        email.To.AddRange(message.To?.Select(a => new MailboxAddress(a, a)) ?? []);
        email.Cc.AddRange(message.Cc?.Select(a => new MailboxAddress(a, a)) ?? []);
        email.Bcc.AddRange(message.Bcc?.Select(a => new MailboxAddress(a, a)) ?? []);
        email.ReplyTo.AddRange(message.ReplyTo?.Select(a => new MailboxAddress(a, a)) ?? []);

        email.Subject = message.Subject;

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = message.HtmlContent,
            TextBody = message.TextContent
        };

        email.Body = bodyBuilder.ToMessageBody();

        return email;
    }
}
