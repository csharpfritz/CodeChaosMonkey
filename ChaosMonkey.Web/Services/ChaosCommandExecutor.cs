using Octokit;
using System.Diagnostics;
using System.Text;

namespace ChaosMonkey.Web.Services;

public class ChaosCommandExecutor
{
	private readonly IConfiguration _configuration;
	private readonly ILogger<ChaosCommandExecutor> _logger;

	// Chaos Monkey agent instructions embedded inline to avoid requiring agent files in destination repos
	private const string ChaosMonkeyInstructions = @"
You are the Chaos Monkey Agent for the St. Jude fundraiser. Your mission is to introduce controlled, entertaining chaos mutations to the codebase.

## Implementation Guidelines

### DO:
- ✅ Keep mutations entertaining but harmless
- ✅ Preserve existing functionality - code should still compile and work
- ✅ Add clear comments explaining what chaos was applied (include 🐒 emoji)
- ✅ Use appropriate humor suitable for live streaming and charity fundraising
- ✅ Target test files primarily for safer mutations
- ✅ Make changes obvious so streamers can easily spot them
- ✅ Test that code compiles after changes

### DON'T:
- ❌ Break the build or cause compilation errors
- ❌ Remove or break existing functionality
- ❌ Use inappropriate language or offensive content
- ❌ Modify critical production code paths
- ❌ Change database connections or external API calls
- ❌ Alter security-related code
";

	public ChaosCommandExecutor(IConfiguration configuration, ILogger<ChaosCommandExecutor> logger)
	{
		_configuration = configuration;
		_logger = logger;
	}

	public async Task<ChaosExecutionResult> ExecuteChaosTaskAsync(Issue issue)
	{
		try
		{
			// Parse the chaos task from the issue body
			var chaosTask = ParseChaosTask(issue);

			if (chaosTask == null)
			{
				return ChaosExecutionResult.Failed("Could not parse chaos task from issue body");
			}

			_logger.LogInformation("Executing chaos task: {Task}", chaosTask.Description);

			// Execute the command based on the task type
			var result = await ExecuteCommandAsync(chaosTask);

			return result;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to execute chaos task for issue #{Number}", issue.Number);
			return ChaosExecutionResult.Failed(ex.Message);
		}
	}

	private ChaosTask? ParseChaosTask(Issue issue)
	{
		// Parse the issue body to extract the chaos task details
		// Expected format in issue body:
		// Chaos Type: [type]
		// Description: [description]
		// Command: [command]

		// log the body for debugging
		_logger.LogInformation("Parsing issue body:\n{Body}", issue.Body);

		var lines = issue.Body?.Split('\n') ?? Array.Empty<string>();

		string? chaosType = null;
		string? description = null;
		string? command = null;
		string requestedBy = "";

		foreach (var line in lines)
		{
			var trimmed = line.Trim();
			if (trimmed.StartsWith("**Instruction**:"))
			{
				description = trimmed.Replace("**Instruction**:", "").Trim();
			}
			else if (trimmed.StartsWith("**Description:**"))
			{
				description = trimmed.Replace("**Description:**", "").Trim();
			}
			else if (trimmed.StartsWith("**Requested by**:"))
			{
				requestedBy = trimmed.Replace("**Requested by**:", "").Trim();
			}
		}

		if (string.IsNullOrWhiteSpace(description))
		{
			return null;
		}

		return new ChaosTask
		{
			Type = chaosType ?? "Unknown",
			Description = description,
			RequestedBy = requestedBy // You can extract this from the issue body if needed
		};
	}

	private async Task<ChaosExecutionResult> ExecuteCommandAsync(ChaosTask task)
	{
		// Get the repository path from configuration
		var repoPath = _configuration["ChaosMonkey:RepositoryPath"];

		if (string.IsNullOrWhiteSpace(repoPath) || !Directory.Exists(repoPath))
		{
			return ChaosExecutionResult.Failed($"Repository path not configured or does not exist: {repoPath}");
		}

		// If a specific command is provided, execute it
		// if (!string.IsNullOrWhiteSpace(task.Command))
		// {
		// 	return await ExecutePowerShellCommandAsync(task.Command, repoPath);
		// }

		// Otherwise, use gh copilot CLI to suggest and execute
		return await ExecuteWithCopilotCLIAsync(task, repoPath);
	}

	private async Task<ChaosExecutionResult> ExecutePowerShellCommandAsync(string command, string workingDirectory)
	{
		try
		{
			var processInfo = new ProcessStartInfo
			{
				FileName = "copilot",
				Arguments = command,
				WorkingDirectory = workingDirectory,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				UseShellExecute = false,
				CreateNoWindow = true
			};

			// log the command being executed
			_logger.LogInformation("Executing PowerShell command: {Command} in {WorkingDirectory}", command, workingDirectory);

			using var process = Process.Start(processInfo);
			if (process == null)
			{
				return ChaosExecutionResult.Failed("Failed to start PowerShell process");
			}

			var output = await process.StandardOutput.ReadToEndAsync();
			var error = await process.StandardError.ReadToEndAsync();

			await process.WaitForExitAsync();

			if (process.ExitCode != 0)
			{
				_logger.LogError("Powershell output: {Output}", output);
				_logger.LogError("Powershell error: {Error}", error);
				return ChaosExecutionResult.Failed($"Command failed with exit code {process.ExitCode}\n{error}");
			}

			return ChaosExecutionResult.Succeeded(output);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to execute PowerShell command: {Command}", command);
			return ChaosExecutionResult.Failed(ex.Message);
		}
	}

	private async Task<ChaosExecutionResult> ExecuteWithCopilotCLIAsync(ChaosTask task, string workingDirectory)
	{
		try
		{
			// Combine the agent instructions with the specific task command
			var fullPrompt = $"{ChaosMonkeyInstructions}\n\n{task.Command}";

			_logger.LogInformation("Invoking Copilot CLI for: {Description}", task.Description);

			// Use ProcessStartInfo.ArgumentList for safer argument handling (no manual escaping needed)
			var processInfo = new ProcessStartInfo
			{
				FileName = "copilot",
				WorkingDirectory = workingDirectory,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				UseShellExecute = false,
				CreateNoWindow = true
			};

			// Add arguments using ArgumentList for automatic and safe escaping
			processInfo.ArgumentList.Add("-p");
			processInfo.ArgumentList.Add(fullPrompt);
			processInfo.ArgumentList.Add("--allow-all-paths");
			processInfo.ArgumentList.Add("--allow-all-tools");
			processInfo.ArgumentList.Add("-s");

			_logger.LogInformation("Executing Copilot CLI in {WorkingDirectory}", workingDirectory);

			using var process = Process.Start(processInfo);
			if (process == null)
			{
				return ChaosExecutionResult.Failed("Failed to start Copilot process");
			}

			var output = await process.StandardOutput.ReadToEndAsync();
			var error = await process.StandardError.ReadToEndAsync();

			await process.WaitForExitAsync();

			if (process.ExitCode != 0)
			{
				_logger.LogError("Copilot output: {Output}", output);
				_logger.LogError("Copilot error: {Error}", error);
				return ChaosExecutionResult.Failed($"Command failed with exit code {process.ExitCode}\n{error}");
			}

			_logger.LogInformation("Copilot CLI completed task: {Description}", task.Description);

			return ChaosExecutionResult.Succeeded(output);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to execute with Copilot CLI");
			return ChaosExecutionResult.Failed(ex.Message);
		}
	}
}

public class ChaosTask
{
	public string Type { get; set; } = string.Empty;
	public string Description { get; set; } = string.Empty;
	public string? Command => $"""
		**Command:** {Description}  This command was auto-generated based on the chaos task description for a donation from {RequestedBy}.  Please apply the requested chaos changes responsibly!
	""";

	public string RequestedBy { get; set; } = string.Empty;
}

public class ChaosExecutionResult
{
	public bool Success { get; set; }
	public string? Output { get; set; }
	public string? Error { get; set; }

	public static ChaosExecutionResult Succeeded(string? output = null)
	{
		return new ChaosExecutionResult
		{
			Success = true,
			Output = output
		};
	}

	public static ChaosExecutionResult Failed(string? error = null)
	{
		return new ChaosExecutionResult
		{
			Success = false,
			Error = error
		};
	}
}
