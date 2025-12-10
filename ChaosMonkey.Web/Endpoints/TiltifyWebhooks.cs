using ChaosMonkey.Web.Models;
using ChaosMonkey.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace ChaosMonkey.Web.Endpoints;

public static class TiltifyWebhooks
{
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
        [FromBody] TiltifyWebhookPayload payload,
        GitHubService gitHubService,
        ILogger<Program> logger)
    {
        try
        {
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

            return Results.Ok();

            // Create GitHub issue for the chaos request
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
}