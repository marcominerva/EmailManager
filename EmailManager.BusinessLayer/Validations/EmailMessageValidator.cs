using EmailManager.Shared.Models;
using FluentValidation;

namespace EmailManager.BusinessLayer.Validations;

public class EmailMessageValidator : AbstractValidator<EmailMessage>
{
    public EmailMessageValidator()
    {
        RuleFor(m => m.SenderEmail).NotEmpty().EmailAddress();
        RuleFor(m => m.Subject).NotEmpty();

        RuleForEach(m => m.To).NotEmpty().EmailAddress();
        RuleForEach(m => m.Cc).NotEmpty().EmailAddress();
        RuleForEach(m => m.Bcc).NotEmpty().EmailAddress();
        RuleForEach(m => m.ReplyTo).NotEmpty().EmailAddress();

        RuleFor(m => m).Must(m => m.To.Any() || m.Cc.Any() || m.Bcc.Any()).WithMessage("At least one recipient is required");
        RuleFor(m => m).Must(m => !string.IsNullOrWhiteSpace(m.HtmlContent) || !string.IsNullOrWhiteSpace(m.TextContent)).WithMessage("Email content is required");
    }
}
