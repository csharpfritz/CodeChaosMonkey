using ChaosMonkey.Web.Models;

namespace ChaosMonkey.Web.Services;

public class ChaosExecutorService : BackgroundService
{
	private readonly IServiceProvider _serviceProvider;
	private readonly ILogger<ChaosExecutorService> _logger;
	private readonly IConfiguration _configuration;
	private readonly TimeSpan _pollInterval;

	public ChaosExecutorService(
		IServiceProvider serviceProvider,
		ILogger<ChaosExecutorService> logger,
		IConfiguration configuration)
	{
		_serviceProvider = serviceProvider;
		_logger = logger;
		_configuration = configuration;
		
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
		var githubService = scope.ServiceProvider.GetRequiredService<GitHubService>();
		var commandExecutor = scope.ServiceProvider.GetRequiredService<ChaosCommandExecutor>();

		var chaosIssues = await githubService.GetUnprocessedChaosIssuesAsync();

		if (chaosIssues.Count == 0)
		{
			_logger.LogDebug("No unprocessed chaos issues found");
			return;
		}

		_logger.LogInformation("Found {Count} unprocessed chaos issue(s)", chaosIssues.Count);

		foreach (var issue in chaosIssues)
		{
			if (cancellationToken.IsCancellationRequested)
				break;

			try
			{
				_logger.LogInformation("Processing chaos issue #{Number}: {Title}", issue.Number, issue.Title);

				// Mark issue as being processed (add a label or comment)
				await githubService.MarkIssueAsProcessingAsync(issue.Number);

				// Execute the chaos command
				var result = await commandExecutor.ExecuteChaosTaskAsync(issue);

				if (result.Success)
				{
					_logger.LogInformation("Successfully executed chaos task for issue #{Number}", issue.Number);
					await githubService.MarkIssueAsCompletedAsync(issue.Number, result.Output);
				}
				else
				{
					_logger.LogError("Failed to execute chaos task for issue #{Number}: {Error}", 
						issue.Number, result.Error);
					await githubService.MarkIssueAsFailedAsync(issue.Number, result.Error);
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error processing chaos issue #{Number}", issue.Number);
				await githubService.MarkIssueAsFailedAsync(issue.Number, ex.Message);
			}
		}
	}
}
