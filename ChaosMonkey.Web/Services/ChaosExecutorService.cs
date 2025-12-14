using ChaosMonkey.Web.Models;
using ChaosMonkey.Web.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace ChaosMonkey.Web.Services;

public class ChaosExecutorService : BackgroundService
{
	private readonly IServiceProvider _serviceProvider;
	private readonly ILogger<ChaosExecutorService> _logger;
	private readonly IConfiguration _configuration;
	private readonly IHubContext<ChaosStatusHub> _hubContext;
	private readonly TimeSpan _pollInterval;

	public ChaosExecutorService(
		IServiceProvider serviceProvider,
		ILogger<ChaosExecutorService> logger,
		IConfiguration configuration,
		IHubContext<ChaosStatusHub> hubContext)
	{
		_serviceProvider = serviceProvider;
		_logger = logger;
		_configuration = configuration;
		_hubContext = hubContext;
		
		// Default to checking every 30 seconds, configurable via appsettings
		var intervalSeconds = _configuration.GetValue<int>("ChaosMonkey:PollIntervalSeconds", 30);
		_pollInterval = TimeSpan.FromSeconds(intervalSeconds);
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		_logger.LogInformation("Chaos Executor Service started. Polling interval: {Interval}", _pollInterval);

		// Wait a bit before starting to allow app to fully initialize
		await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);

		while (!stoppingToken.IsCancellationRequested)
		{
			try
			{
				await ProcessChaosIssuesAsync(stoppingToken);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error processing chaos issues");
			}

			await Task.Delay(_pollInterval, stoppingToken);
		}

		_logger.LogInformation("Chaos Executor Service stopped");
	}

	private async Task ProcessChaosIssuesAsync(CancellationToken cancellationToken)
	{
		// Create a scope to resolve scoped services
		using var scope = _serviceProvider.CreateScope();
		var jsonQueueService = scope.ServiceProvider.GetRequiredService<JsonQueueService>();
		var commandExecutor = scope.ServiceProvider.GetRequiredService<ChaosCommandExecutor>();

		var queueItems = await jsonQueueService.GetPendingItemsAsync();

		if (queueItems.Count == 0)
		{
			_logger.LogDebug("No pending queue items found");
			return;
		}

		_logger.LogInformation("Found {Count} pending queue item(s)", queueItems.Count);

		foreach (var queueItem in queueItems)
		{
			if (cancellationToken.IsCancellationRequested)
				break;

			try
			{
				_logger.LogInformation("Processing queue item {Id}: {Description}", queueItem.Id, queueItem.Description);

				// Mark item as being processed
				await jsonQueueService.MarkAsProcessingAsync(queueItem.Id);

				// Notify overlay that chaos is starting
				await _hubContext.Clients.All.SendAsync("ChaosStarted", queueItem.Id, queueItem.Description, cancellationToken);

				// Execute the chaos command
				var result = await commandExecutor.ExecuteChaosTaskAsync(queueItem);

				if (result.Success)
				{
					_logger.LogInformation("Successfully executed chaos task for queue item {Id}", queueItem.Id);
					await jsonQueueService.MarkAsCompletedAsync(queueItem.Id, result.Output);
					await _hubContext.Clients.All.SendAsync("ChaosCompleted", queueItem.Id, true, cancellationToken);
				}
				else
				{
					_logger.LogError("Failed to execute chaos task for queue item {Id}: {Error}", 
						queueItem.Id, result.Error);
					await jsonQueueService.MarkAsFailedAsync(queueItem.Id, result.Error);
					await _hubContext.Clients.All.SendAsync("ChaosCompleted", queueItem.Id, false, cancellationToken);
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error processing queue item {Id}", queueItem.Id);
				await jsonQueueService.MarkAsFailedAsync(queueItem.Id, ex.Message);
				await _hubContext.Clients.All.SendAsync("ChaosCompleted", queueItem.Id, false, cancellationToken);
			}
		}
	}
}
