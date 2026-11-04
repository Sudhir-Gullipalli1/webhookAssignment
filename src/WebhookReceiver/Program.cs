using System.Text.Json;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using WebhookReceiver;
using WebhookReceiver.Data;
using WebhookReceiver.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Webhooks")));

var app = builder.Build();

var webhookSecret = app.Configuration["Stripe:WebhookSecret"];
if (string.IsNullOrEmpty(webhookSecret))
    throw new InvalidOperationException("Stripe:WebhookSecret is not configured");

app.MapPost("/webhooks/stripe", async (HttpRequest request, AppDbContext db, ILogger<Program> logger) =>
{
    using var reader = new StreamReader(request.Body);
    var body = await reader.ReadToEndAsync();

    var signature = request.Headers["Stripe-Signature"].ToString();
    if (!StripeSignatureVerifier.Verify(body, signature, webhookSecret))
    {
        logger.LogWarning("Rejected webhook: invalid signature");
        return Results.BadRequest(new { error = "Invalid signature" });
    }

    StripeEventRequest? stripeEvent;
    try
    {
        stripeEvent = JsonSerializer.Deserialize<StripeEventRequest>(body);
    }
    catch (JsonException)
    {
        return Results.BadRequest(new { error = "Malformed JSON" });
    }

    if (stripeEvent is null)
        return Results.BadRequest(new { error = "Invalid event payload" });

    if (!ModelValidator.TryValidate(stripeEvent, out var errors))
        return Results.ValidationProblem(errors);

    var webhookEvent = new WebhookEvent
    {
        EventId = stripeEvent.Id!,
        Type = stripeEvent.Type!,
        Livemode = stripeEvent.Livemode ?? false,
        CreatedAtUtc = stripeEvent.Created is long created
            ? DateTimeOffset.FromUnixTimeSeconds(created).UtcDateTime
            : null,
        ReceivedAtUtc = DateTime.UtcNow,
        Payload = body
    };

    if (await db.WebhookEvents.AnyAsync(e => e.EventId == webhookEvent.EventId))
    {
        logger.LogInformation("Duplicate webhook {EventId} ignored", webhookEvent.EventId);
        return Results.Ok(new { received = true, duplicate = true });
    }

    db.WebhookEvents.Add(webhookEvent);
    try
    {
        await db.SaveChangesAsync();
    }
    catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2627 or 2601 })
    {
        logger.LogInformation("Duplicate webhook {EventId} ignored", webhookEvent.EventId);
        return Results.Ok(new { received = true, duplicate = true });
    }

    logger.LogInformation("Stored webhook {EventId} ({Type})", webhookEvent.EventId, webhookEvent.Type);
    return Results.Ok(new { received = true, duplicate = false });
});

app.Run();
