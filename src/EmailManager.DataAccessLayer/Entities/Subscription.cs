namespace EmailManager.DataAccessLayer.Entities;

public class Subscription
{
    public Guid Id { get; set; }

    public string UserName { get; set; } = null!;

    public string ApiKey { get; set; } = null!;

    public DateTimeOffset ValidFrom { get; set; }

    public DateTimeOffset ValidTo { get; set; }

    public int RequestsPerWindow { get; set; }

    public int WindowMinutes { get; set; }
}