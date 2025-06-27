using System.Globalization;
using System.Threading.RateLimiting;
using EmailManager.BusinessLayer.Services;
using EmailManager.BusinessLayer.Services.Interfaces;
using EmailManager.BusinessLayer.Validations;
using EmailManager.Shared.Models;
using FluentValidation;
using Microsoft.AspNetCore.Localization;
using MinimalHelpers.FluentValidation;
using TinyHelpers.AspNetCore.Extensions;
using TinyHelpers.AspNetCore.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddKeyedSingleton<IEmailService, SmtpEmailService>("smtp");
//builder.Services.AddKeyedSingleton<IEmailService, SendgridEmailService>("sendgrid");

builder.Services.AddRequestLocalization(options =>
{
    var supportedCultures = CultureInfo.GetCultures(CultureTypes.SpecificCultures);
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
    options.DefaultRequestCulture = new RequestCulture("en-US");
});

builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("SendEmail", context =>
    {
        var permitLimit = int.TryParse(context.User.Claims.FirstOrDefault(c => c.Type == "requests_per_window")?.Value, out var requestPerWindow) ? requestPerWindow : 3;
        var window = int.TryParse(context.User.Claims.FirstOrDefault(c => c.Type == "window_minutes")?.Value, out var windowMinutes) ? TimeSpan.FromMinutes(windowMinutes) : TimeSpan.FromMinutes(1);

        return RateLimitPartition.GetFixedWindowLimiter(context.User.Identity?.Name ?? "Default", _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = permitLimit,
            Window = window,
            QueueLimit = 0,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst
        });
    });

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.OnRejected = (context, _) =>
    {
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var window))
        {
            var response = context.HttpContext.Response;
            response.Headers.RetryAfter = window.TotalSeconds.ToString();
        }

        return ValueTask.CompletedTask;
    };
});

ValidatorOptions.Global.LanguageManager.Enabled = false;
builder.Services.AddValidatorsFromAssemblyContaining<EmailMessageValidator>();

builder.Services.AddOpenApi(options =>
{
    options.RemoveServerList();

    options.AddDefaultProblemDetailsResponse();
    options.AddAcceptLanguageHeader();
});

builder.Services.AddDefaultProblemDetails();
builder.Services.AddDefaultExceptionHandler();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();

app.UseExceptionHandler();
app.UseStatusCodePages();

app.MapOpenApi();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/openapi/v1.json", app.Environment.ApplicationName);
});

app.UseRouting();
//app.UseCors();

app.UseRequestLocalization();

//app.UseAuthentication();
//app.UseAuthorization();

app.UseRateLimiter();

app.MapPost("/api/{service:regex(smtp)}", async (string service, IServiceProvider serviceProvider, EmailMessage emailMessage, CancellationToken cancellationToken) =>
{
    var emailService = serviceProvider.GetRequiredKeyedService<IEmailService>(service);
    var response = await emailService.SendAsync(emailMessage, cancellationToken);

    return TypedResults.Ok(response);
})
.Produces<SendEmailResult>()
.WithValidation<EmailMessage>()
.RequireRateLimiting("SendEmail");

app.Run();
