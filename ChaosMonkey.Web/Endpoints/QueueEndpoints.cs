using ChaosMonkey.Web.Services;

namespace ChaosMonkey.Web.Endpoints;

public static class QueueEndpoints
{
    public static void MapQueueEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/queue/status", GetQueueStatus)
            .WithName("GetQueueStatus")
            .WithTags("Queue");
    }

    private static async Task<IResult> GetQueueStatus(
        JsonQueueService jsonQueueService,
        ILogger<Program> logger)
    {
        try
        {
            var pendingItems = await jsonQueueService.GetPendingItemsAsync();

            return Results.Ok(new
            {
                Status = "OK",
                PendingCount = pendingItems.Count,
                Items = pendingItems.Select(item => new
                {
                    item.Id,
                    item.DonationId,
                    item.DonorName,
                    item.Amount,
                    item.Currency,
                    item.Description,
                    item.ChaosType,
                    item.Status,
                    item.CreatedAt
                })
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting queue status");
            return Results.Json(
                new { Status = "Error", Message = "Failed to get queue status" },
                statusCode: 500
            );
        }
    }
}
