using ChaosMonkey.Web.Models;
using ChaosMonkey.Web.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;

namespace ChaosMonkey.Web.Endpoints;

public static class TiltifyWebhooks
{
    // 🐒 Chaos Monkey: renamed to a pun 'MapTiltifyEndpuns' for streaming fun (Donor: elliface, DonationID: N/A)
    public static void MapTiltifyEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/webhooks/tiltify", HandleTiltifyWebhook)
            .WithName("TiltifyWebhook")
            .WithTags("Webhooks");

        endpoints.MapGet("/webhooks/tiltify/health", () => new { Status = "OK", Service = "Tiltify Webhook Handler" })
            .WithName("TiltifyWebhookHealth")
            .WithTags("Health");
    }

    private static async Task<IResult> HandleTiltifyWebhook(
        HttpContext httpContext,
        [FromBody] TiltifyWebhookPayload payload,
        [FromServices] IConfiguration configuration,
        GitHubService gitHubService,
        ILogger<Program> logger)
    {
        try
        {
            // Verify webhook signature
            if (!await VerifyTiltifySignature(httpContext, configuration, logger))
            {
                logger.LogWarning("Invalid Tiltify webhook signature");
                return Results.Json(
                    new WebhookResponse(false, "Invalid signature"),
                    statusCode: 401
                );
            }

            logger.LogInformation("Received Tiltify webhook: {EventType} for donation {DonationId}",
                payload.Meta.Event_Type, payload.Data.Id);

            // Log donation details including custom questions
            var donationAmount = decimal.Parse(payload.Data.Amount.Value);
            logger.LogInformation("Donation Details: Amount: {Currency} ${Amount}, Donor: {DonorName}, Custom Question: {CustomQuestion}",
                payload.Data.Amount.Currency, donationAmount, payload.Data.Donor_Name,
                payload.Data.Reward_Custom_Question ?? "None");

            // Only process donation events
            if (!payload.Meta.Event_Type.Contains("donation"))
            {
                logger.LogInformation("Ignoring non-donation webhook type: {EventType}", payload.Meta.Event_Type);
                return Results.Ok(new WebhookResponse(true, $"Ignored webhook type: {payload.Meta.Event_Type}"));
            }

            // decimal donationAmountValue = payload.Data.Amount.Value != "" ? decimal.Parse(payload.Data.Amount.Value) : 0m;
            // var minimumDonationValue = configuration.GetValue<decimal>("ChaosMonkey:MinimumDonationThreshold", 5.0m);
            // // if (donationAmountValue > minimumDonationValue || donationAmountValue < 1.0m) {
            // 	logger.LogInformation("Ignoring donation amount: {DonationAmount}", donationAmountValue);
            // 	return Results.Ok(new WebhookResponse(true, $"Ignored donation amount: {donationAmountValue}"));
            // }

            // Create GitHub issue for the chaos request
            logger.LogInformation($"Payload: {System.Text.Json.JsonSerializer.Serialize(payload.Data)}");
            var issueUrl = await gitHubService.CreateChaosIssueAsync(payload.Data);

            if (issueUrl != null)
            {
                var response = new WebhookResponse(
                    true,
                    "Successfully created chaos issue",
                    issueUrl
                );

                var amount = decimal.Parse(payload.Data.Amount.Value);
                logger.LogInformation("Successfully processed donation {DonationId} from {DonorName} ({Currency} ${Amount})",
                    payload.Data.Id, payload.Data.Donor_Name, payload.Data.Amount.Currency, amount);

                return Results.Ok(response);
            }
            else
            {
                logger.LogWarning("Failed to create GitHub issue for donation {DonationId}", payload.Data.Id);
                return Results.Json(
                    new WebhookResponse(false, "Failed to create GitHub issue"),
                    statusCode: 500
                );
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing Tiltify webhook");
            return Results.Json(
                new WebhookResponse(false, "Internal server error"),
                statusCode: 500
            );
        }
    }

    private static async Task<bool> VerifyTiltifySignature(
        HttpContext context,
        IConfiguration configuration,
        ILogger logger)
    {
        var webhookSecret = configuration["Tiltify:WebhookSecret"];
        
        // Allow skipping signature verification in development
        if (string.IsNullOrWhiteSpace(webhookSecret))
        {
            logger.LogWarning("Tiltify:WebhookSecret not configured - skipping signature verification (NOT SECURE!)");
            return true;
        }

        // Get the signature from headers
        if (!context.Request.Headers.TryGetValue("X-Tiltify-Signature", out var signatureHeader))
        {
            logger.LogWarning("Missing X-Tiltify-Signature header");
            return false;
        }

        var receivedSignature = signatureHeader.ToString();
        
        // Read the raw body for signature verification
        context.Request.EnableBuffering();
        context.Request.Body.Position = 0;
        
        using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
        var rawBody = await reader.ReadToEndAsync();
        context.Request.Body.Position = 0;

        // Compute HMAC-SHA256 signature
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(webhookSecret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(rawBody));
        var computedSignature = Convert.ToHexString(hash).ToLowerInvariant();

        // Compare signatures (constant time comparison to prevent timing attacks)
        var isValid = CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(receivedSignature),
            Encoding.UTF8.GetBytes(computedSignature)
        );

        if (!isValid)
        {
            logger.LogWarning("Signature mismatch. Received: {Received}, Computed: {Computed}",
                receivedSignature, computedSignature);
        }

        return isValid;
    }
}