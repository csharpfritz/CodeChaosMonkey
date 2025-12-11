using Octokit;
using ChaosMonkey.Web.Models;

namespace ChaosMonkey.Web.Services;

public class GitHubService
{
	private readonly GitHubClient _client;
	private readonly IConfiguration _configuration;
	private readonly ILogger<GitHubService> _logger;

	public GitHubService(IConfiguration configuration, ILogger<GitHubService> logger)
	{
		_configuration = configuration;
		_logger = logger;

		var token = _configuration["GitHub:Token"];
		if (string.IsNullOrWhiteSpace(token))
		{
			throw new InvalidOperationException("GitHub token is required. Set GitHub:Token in configuration.");
		}

		_client = new GitHubClient(new ProductHeaderValue("chaos-monkey"))
		{
			Credentials = new Credentials(token)
		};
	}

	public async Task<string?> CreateChaosIssueAsync(TiltifyDonationData donation)
	{
		try
		{
			var owner = _configuration["GitHub:Owner"] ?? throw new InvalidOperationException("GitHub:Owner is required");
			var repo = _configuration["GitHub:Repository"] ?? throw new InvalidOperationException("GitHub:Repository is required");

			_logger.LogInformation("Creating GitHub issue in {Owner}/{Repo} for donation {DonationId}", owner, repo, donation?.Id ?? "null");

			var title = ChaosMapper.GenerateIssueTitle(donation);

			var body = ChaosMapper.GenerateIssueBody(donation);

			var newIssue = null as NewIssue;
			newIssue = new NewIssue(title)
			{
				Body = body
			};

			newIssue.Labels.Add("chaos");
			newIssue.Labels.Add("donation");

			var issue = await _client.Issue.Create(owner, repo, newIssue);

			_logger.LogInformation("Created GitHub issue #{IssueNumber} for donation {DonationId} from {DonorName}",
					issue?.Number ?? 0, donation.Id, donation.Donor_Name);

			return issue.HtmlUrl;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to create GitHub issue for donation {DonationId}", donation.Id);
			return null;
		}
	}

	public async Task<List<Issue>> GetUnprocessedChaosIssuesAsync()
	{
		try
		{
			var owner = _configuration["GitHub:Owner"] ?? throw new InvalidOperationException("GitHub:Owner is required");
			var repo = _configuration["GitHub:Repository"] ?? throw new InvalidOperationException("GitHub:Repository is required");

			var issueRequest = new RepositoryIssueRequest
			{
				State = ItemStateFilter.Open,
				Labels = { "chaos" }
			};

			var allIssues = await _client.Issue.GetAllForRepository(owner, repo, issueRequest);

			// Filter out issues that have already been processed (have "processing" or "completed" labels)
			var unprocessed = allIssues
				.Where(i => !i.Labels.Any(l => l.Name == "processing" || l.Name == "completed" || l.Name == "failed"))
				.ToList();

			_logger.LogDebug("Found {Count} unprocessed chaos issues out of {Total} total", 
				unprocessed.Count, allIssues.Count);

			// Fetch full issue details to get the complete body content
			var fullIssues = new List<Issue>();
			foreach (var issue in unprocessed)
			{
				var fullIssue = await _client.Issue.Get(owner, repo, issue.Number);
				fullIssues.Add(fullIssue);
			}

			return fullIssues;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to fetch chaos issues");
			return new List<Issue>();
		}
	}

	public async Task MarkIssueAsProcessingAsync(int issueNumber)
	{
		try
		{
			var owner = _configuration["GitHub:Owner"] ?? throw new InvalidOperationException("GitHub:Owner is required");
			var repo = _configuration["GitHub:Repository"] ?? throw new InvalidOperationException("GitHub:Repository is required");

			var update = new IssueUpdate();
			update.AddLabel("processing");

			await _client.Issue.Update(owner, repo, issueNumber, update);

			// Add a comment to indicate processing has started
			await _client.Issue.Comment.Create(owner, repo, issueNumber, 
				"🤖 Chaos Monkey is processing this task...");

			_logger.LogInformation("Marked issue #{IssueNumber} as processing", issueNumber);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to mark issue #{IssueNumber} as processing", issueNumber);
		}
	}

	public async Task MarkIssueAsCompletedAsync(int issueNumber, string? output)
	{
		try
		{
			var owner = _configuration["GitHub:Owner"] ?? throw new InvalidOperationException("GitHub:Owner is required");
			var repo = _configuration["GitHub:Repository"] ?? throw new InvalidOperationException("GitHub:Repository is required");

			var update = new IssueUpdate
			{
				State = ItemState.Closed
			};
			update.AddLabel("completed");
			update.RemoveLabel("processing");

			await _client.Issue.Update(owner, repo, issueNumber, update);

			// Add a comment with the result
			var comment = $"✅ Chaos task completed!\n\n";
			if (!string.IsNullOrWhiteSpace(output))
			{
				comment += $"```\n{output}\n```";
			}

			await _client.Issue.Comment.Create(owner, repo, issueNumber, comment);

			_logger.LogInformation("Marked issue #{IssueNumber} as completed", issueNumber);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to mark issue #{IssueNumber} as completed", issueNumber);
		}
	}

	public async Task MarkIssueAsFailedAsync(int issueNumber, string? error)
	{
		try
		{
			var owner = _configuration["GitHub:Owner"] ?? throw new InvalidOperationException("GitHub:Owner is required");
			var repo = _configuration["GitHub:Repository"] ?? throw new InvalidOperationException("GitHub:Repository is required");

			var update = new IssueUpdate();
			update.AddLabel("failed");
			update.RemoveLabel("processing");

			await _client.Issue.Update(owner, repo, issueNumber, update);

			// Add a comment with the error
			var comment = $"❌ Chaos task failed!\n\n";
			if (!string.IsNullOrWhiteSpace(error))
			{
				comment += $"Error: {error}";
			}

			await _client.Issue.Comment.Create(owner, repo, issueNumber, comment);

			_logger.LogError("Marked issue #{IssueNumber} as failed", issueNumber);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to mark issue #{IssueNumber} as failed", issueNumber);
		}
	}
}