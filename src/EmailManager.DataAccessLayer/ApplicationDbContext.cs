using System.Reflection;
using EmailManager.DataAccessLayer.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace EmailManager.DataAccessLayer;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IConfiguration configuration) : DbContext(options)
{
    public DbSet<Subscription> Subscriptions { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        var administratorUserName = configuration.GetValue<string>("AppSettings:AdministratorUserName")!;
        var administratorApiKey = configuration.GetValue<string>("AppSettings:AdministratorApiKey")!;

        optionsBuilder.UseSeeding((context, _) =>
        {
            var subscription = context.Set<Subscription>().FirstOrDefault(s => s.UserName == administratorUserName);
            CheckSubscription(context, subscription, administratorUserName, administratorApiKey);

            context.SaveChanges();
        });

        optionsBuilder.UseAsyncSeeding(async (context, _, cancellationToken) =>
        {
            var subscription = await context.Set<Subscription>().FirstOrDefaultAsync(s => s.UserName == administratorUserName, cancellationToken: cancellationToken);
            CheckSubscription(context, subscription, administratorUserName, administratorApiKey);

            await context.SaveChangesAsync(cancellationToken);
        });

        static void CheckSubscription(DbContext context, Subscription? subscription, string administratorUserName, string administratorApiKey)
        {
            if (subscription is null)
            {
                context.Add(new Subscription
                {
                    UserName = administratorUserName,
                    ApiKey = administratorApiKey,
                    RequestsPerWindow = 10,
                    WindowMinutes = 1,
                    ValidFrom = DateTimeOffset.MinValue,
                    ValidTo = DateTimeOffset.MaxValue
                });
            }
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}