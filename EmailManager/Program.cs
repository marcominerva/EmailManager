using EmailManager.BusinessLayer.Services;
using EmailManager.BusinessLayer.Services.Interfaces;
using EmailManager.BusinessLayer.Validations;
using EmailManager.Shared.Models;
using FluentValidation;
using MinimalHelpers.FluentValidation;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddKeyedSingleton<IEmailService, SmtpEmailService>("smtp");
//builder.Services.AddKeyedSingleton<IEmailService, SendgridEmailService>("sendgrid");

builder.Services.AddValidatorsFromAssemblyContaining<EmailMessageValidator>();

builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();

app.MapOpenApi();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/openapi/v1.json", app.Environment.ApplicationName);
});

app.MapPost("/api/{service:regex(smtp)}", async (string service, IServiceProvider serviceProvider, EmailMessage emailMessage, CancellationToken cancellationToken) =>
{
    var emailService = serviceProvider.GetRequiredKeyedService<IEmailService>(service);
    var response = await emailService.SendAsync(emailMessage, cancellationToken);

    return TypedResults.Ok(response);
})
.WithValidation<EmailMessage>();

app.Run();
