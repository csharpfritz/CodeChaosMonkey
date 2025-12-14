namespace ChaosMonkey.Web.Models;

public class ChaosQueueItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string DonationId { get; set; } = string.Empty;
    public string DonorName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public string Description { get; set; } = string.Empty;
    public string ChaosType { get; set; } = string.Empty;
    public ChaosQueueStatus Status { get; set; } = ChaosQueueStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }
    public string? Output { get; set; }
    public string? Error { get; set; }
}

public enum ChaosQueueStatus
{
    Pending,
    Processing,
    Completed,
    Failed
}
