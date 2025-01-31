using System.Net;

namespace EmailManager.Shared.Models;

public class SendEmailResult
{
    public int StatusCode { get; set; } = (int)HttpStatusCode.OK;

    public string? MessageId { get; set; }

    public string? ErrorMessage { get; set; }

    public SendEmailResult()
    {
    }

    public SendEmailResult(int statusCode, string messageId)
    {
        StatusCode = statusCode;
        MessageId = messageId;
    }

    public SendEmailResult(Exception error)
    {
        StatusCode = (int)HttpStatusCode.InternalServerError;
        ErrorMessage = error.Message;
    }
}
