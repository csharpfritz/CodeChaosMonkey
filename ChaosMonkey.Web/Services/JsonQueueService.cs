using ChaosMonkey.Web.Models;
using System.Text.Json;

namespace ChaosMonkey.Web.Services;

public class JsonQueueService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<JsonQueueService> _logger;
    private readonly string _queueFilePath;
    private readonly SemaphoreSlim _fileLock = new(1, 1);

    public JsonQueueService(IConfiguration configuration, ILogger<JsonQueueService> logger)
    {
        _configuration = configuration;
        _logger = logger;

        // Get queue file path from configuration or use default
        var defaultPath = Path.Combine(AppContext.BaseDirectory, "chaos-queue.json");
        _queueFilePath = _configuration["ChaosMonkey:QueueFilePath"] ?? defaultPath;

        _logger.LogInformation("JsonQueueService initialized with queue file: {QueueFilePath}", _queueFilePath);

        // Ensure the queue file exists
        EnsureQueueFileExists();
    }

    private void EnsureQueueFileExists()
    {
        if (!File.Exists(_queueFilePath))
        {
            _logger.LogInformation("Queue file does not exist, creating new file at {QueueFilePath}", _queueFilePath);
            var emptyQueue = new List<ChaosQueueItem>();
            File.WriteAllText(_queueFilePath, JsonSerializer.Serialize(emptyQueue, new JsonSerializerOptions 
            { 
                WriteIndented = true 
            }));
        }
    }

    public async Task<string> AddToQueueAsync(TiltifyDonationData donation)
    {
        try
        {
            var amount = decimal.Parse(donation.Amount.Value);
            var instruction = ChaosMapper.MapToInstruction(amount);

            var queueItem = new ChaosQueueItem
            {
                DonationId = donation.Id,
                DonorName = donation.Donor_Name,
                Amount = amount,
                Currency = donation.Amount.Currency,
                Description = instruction,
                ChaosType = amount >= 5 ? "Complex" : "Simple",
                Status = ChaosQueueStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            await _fileLock.WaitAsync();
            try
            {
                var queue = await ReadQueueAsync();
                queue.Add(queueItem);
                await WriteQueueAsync(queue);

                _logger.LogInformation("Added donation {DonationId} from {DonorName} to queue with ID {QueueId}",
                    donation.Id, donation.Donor_Name, queueItem.Id);

                return queueItem.Id;
            }
            finally
            {
                _fileLock.Release();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add donation {DonationId} to queue", donation.Id);
            throw;
        }
    }

    public async Task<List<ChaosQueueItem>> GetPendingItemsAsync()
    {
        try
        {
            await _fileLock.WaitAsync();
            try
            {
                var queue = await ReadQueueAsync();
                var pending = queue.Where(item => item.Status == ChaosQueueStatus.Pending).ToList();

                _logger.LogDebug("Found {Count} pending items in queue", pending.Count);

                return pending;
            }
            finally
            {
                _fileLock.Release();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get pending items from queue");
            return new List<ChaosQueueItem>();
        }
    }

    public async Task MarkAsProcessingAsync(string queueItemId)
    {
        try
        {
            await _fileLock.WaitAsync();
            try
            {
                var queue = await ReadQueueAsync();
                var item = queue.FirstOrDefault(i => i.Id == queueItemId);

                if (item != null)
                {
                    item.Status = ChaosQueueStatus.Processing;
                    await WriteQueueAsync(queue);

                    _logger.LogInformation("Marked queue item {QueueItemId} as processing", queueItemId);
                }
                else
                {
                    _logger.LogWarning("Queue item {QueueItemId} not found", queueItemId);
                }
            }
            finally
            {
                _fileLock.Release();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to mark queue item {QueueItemId} as processing", queueItemId);
        }
    }

    public async Task MarkAsCompletedAsync(string queueItemId, string? output)
    {
        try
        {
            await _fileLock.WaitAsync();
            try
            {
                var queue = await ReadQueueAsync();
                var item = queue.FirstOrDefault(i => i.Id == queueItemId);

                if (item != null)
                {
                    item.Status = ChaosQueueStatus.Completed;
                    item.ProcessedAt = DateTime.UtcNow;
                    item.Output = output;
                    await WriteQueueAsync(queue);

                    _logger.LogInformation("Marked queue item {QueueItemId} as completed", queueItemId);
                }
                else
                {
                    _logger.LogWarning("Queue item {QueueItemId} not found", queueItemId);
                }
            }
            finally
            {
                _fileLock.Release();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to mark queue item {QueueItemId} as completed", queueItemId);
        }
    }

    public async Task MarkAsFailedAsync(string queueItemId, string? error)
    {
        try
        {
            await _fileLock.WaitAsync();
            try
            {
                var queue = await ReadQueueAsync();
                var item = queue.FirstOrDefault(i => i.Id == queueItemId);

                if (item != null)
                {
                    item.Status = ChaosQueueStatus.Failed;
                    item.ProcessedAt = DateTime.UtcNow;
                    item.Error = error;
                    await WriteQueueAsync(queue);

                    _logger.LogError("Marked queue item {QueueItemId} as failed", queueItemId);
                }
                else
                {
                    _logger.LogWarning("Queue item {QueueItemId} not found", queueItemId);
                }
            }
            finally
            {
                _fileLock.Release();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to mark queue item {QueueItemId} as failed", queueItemId);
        }
    }

    private async Task<List<ChaosQueueItem>> ReadQueueAsync()
    {
        try
        {
            var json = await File.ReadAllTextAsync(_queueFilePath);
            return JsonSerializer.Deserialize<List<ChaosQueueItem>>(json) ?? new List<ChaosQueueItem>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read queue file, returning empty queue");
            return new List<ChaosQueueItem>();
        }
    }

    private async Task WriteQueueAsync(List<ChaosQueueItem> queue)
    {
        try
        {
            var json = JsonSerializer.Serialize(queue, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            await File.WriteAllTextAsync(_queueFilePath, json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to write queue file");
            throw;
        }
    }
}
