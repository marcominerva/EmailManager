using EmailManager.DataAccessLayer.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmailManager.DataAccessLayer.Configurations;

internal class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> builder)
    {
        builder.ToTable("Subscriptions");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasDefaultValueSql("(newsequentialid())");

        builder.HasIndex(e => e.ApiKey, "IX_Subscriptions_ApiKey").IsUnique();
        builder.HasIndex(e => e.UserName, "IX_Subscriptions_UserName").IsUnique();

        builder.Property(e => e.ApiKey).HasMaxLength(512).IsUnicode(false);
        builder.Property(e => e.UserName).HasMaxLength(255);
    }
}