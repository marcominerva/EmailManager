using System.Security.Claims;
using EmailManager.DataAccessLayer;
using Microsoft.EntityFrameworkCore;
using SimpleAuthentication.ApiKey;

namespace EmailManager.BusinessLayer.Authentication;

public class SubscriptionValidator(ApplicationDbContext dbContext, TimeProvider timeProvider) : IApiKeyValidator
{
    public async Task<ApiKeyValidationResult> ValidateAsync(string apiKey)
    {
        var subscription = await dbContext.Subscriptions.FirstOrDefaultAsync(s => s.ApiKey == apiKey);
        if (subscription is null)
        {
            return ApiKeyValidationResult.Fail("API key is invalid");
        }

        var now = timeProvider.GetUtcNow();
        if (subscription.ValidFrom > now || subscription.ValidTo < now)
        {
            return ApiKeyValidationResult.Fail("API key is expired");
        }

        var claims = new List<Claim>
        {
            new("requests_per_window", subscription.RequestsPerWindow.ToString()),
            new("window_minutes", subscription.WindowMinutes.ToString())
        };

        return ApiKeyValidationResult.Success(subscription.UserName, claims);
    }
}