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
        // if (string.IsNullOrWhiteSpace(token))
        // {
        //     throw new InvalidOperationException("GitHub token is required. Set GitHub:Token in configuration.");
        // }

        // _client = new GitHubClient(new ProductHeaderValue("chaos-monkey"))
        // {
        //     Credentials = new Credentials(token)
        // };
    }

    public async Task<string?> CreateChaosIssueAsync(TiltifyDonationData donation)
    {
        try
        {
            var owner = _configuration["GitHub:Owner"] ?? throw new InvalidOperationException("GitHub:Owner is required");
            var repo = _configuration["GitHub:Repository"] ?? throw new InvalidOperationException("GitHub:Repository is required");

            var title = ChaosMapper.GenerateIssueTitle(donation);
            var body = ChaosMapper.GenerateIssueBody(donation);

            var newIssue = new NewIssue(title)
            {
                Body = body
            };
            
            newIssue.Labels.Add("chaos");
            newIssue.Labels.Add("donation");

            var issue = await _client.Issue.Create(owner, repo, newIssue);
            
            _logger.LogInformation("Created GitHub issue #{IssueNumber} for donation {DonationId} from {DonorName}", 
                issue.Number, donation.Id, donation.Donor_Name);

            return issue.HtmlUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create GitHub issue for donation {DonationId}", donation.Id);
            return null;
        }
    }
}