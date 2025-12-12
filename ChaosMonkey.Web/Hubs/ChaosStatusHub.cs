using Microsoft.AspNetCore.SignalR;

namespace ChaosMonkey.Web.Hubs;

public class ChaosStatusHub : Hub
{
	private readonly ILogger<ChaosStatusHub> _logger;

	public ChaosStatusHub(ILogger<ChaosStatusHub> logger)
	{
		_logger = logger;
	}

	public override async Task OnConnectedAsync()
	{
		_logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);
		await base.OnConnectedAsync();
	}

	public override async Task OnDisconnectedAsync(Exception? exception)
	{
		_logger.LogInformation("Client disconnected: {ConnectionId}", Context.ConnectionId);
		await base.OnDisconnectedAsync(exception);
	}
}
