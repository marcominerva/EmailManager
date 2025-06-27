namespace EmailManager.BusinessLayer.Settings;

public class SmtpSettings
{
    public string Host { get; init; } = string.Empty;

    public int Port { get; init; }

    public bool UseSsl { get; init; }

    public string? UserName { get; init; }

    public string? Password { get; init; }

    public bool IgnoreServerCertificateErrors { get; init; }
}
