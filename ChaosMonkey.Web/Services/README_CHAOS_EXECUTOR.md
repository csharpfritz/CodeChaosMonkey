# Chaos Executor Background Service

## Overview

The Chaos Executor is a background service that runs within the ChaosMonkey.Web application. It automatically polls GitHub for new chaos issues and executes them locally on your workstation.

## How It Works

1. **Issue Creation**: When a donation comes in via Tiltify webhook, a GitHub issue is created with a "chaos" label
2. **Background Polling**: The `ChaosExecutorService` polls GitHub every 15-30 seconds (configurable) for unprocessed chaos issues
3. **Task Execution**: When found, the `ChaosCommandExecutor` parses the issue and executes the chaos task locally
4. **Status Updates**: The service updates the issue with status labels and comments:
   - `processing` - Task is being executed
   - `completed` - Task finished successfully
   - `failed` - Task encountered an error

## Configuration

Add these settings to your `appsettings.json`:

```json
{
  "ChaosMonkey": {
    "PollIntervalSeconds": 30,
    "RepositoryPath": "D:\\CodeChaosMonkey"
  }
}
```

- **PollIntervalSeconds**: How often to check for new chaos issues (default: 30 seconds, Dev: 15 seconds)
- **RepositoryPath**: Absolute path to your repository where chaos commands will be executed

## Components

### ChaosExecutorService
- Background service (`IHostedService`) that runs continuously
- Polls GitHub for unprocessed chaos issues
- Orchestrates the execution pipeline

### GitHubService (Extended)
New methods added:
- `GetUnprocessedChaosIssuesAsync()` - Fetches open issues with "chaos" label that haven't been processed
- `MarkIssueAsProcessingAsync()` - Adds "processing" label and comment
- `MarkIssueAsCompletedAsync()` - Closes issue, adds "completed" label and success comment
- `MarkIssueAsFailedAsync()` - Adds "failed" label and error comment

### ChaosCommandExecutor
- Parses chaos tasks from GitHub issue bodies
- Executes PowerShell commands in the repository directory
- Can integrate with `gh copilot suggest` for AI-assisted command generation
- **Embeds Chaos Monkey agent instructions inline** - no need to copy agent files to destination repositories
- Returns execution results (success/failure with output)

## Issue Body Format

The executor expects issue bodies in this format:

```
**Chaos Type:** Simple|Complex
**Description:** [What to do]
**Command:** [Optional PowerShell command]
```

The `ChaosMapper` automatically generates issues in this format.

## Safety Features

1. **Auto-execution is disabled by default** for Copilot CLI suggestions - only logs what would be executed
2. Tasks run in isolated PowerShell processes
3. All execution attempts are logged
4. Failed tasks are marked and reported
5. Repository path must be explicitly configured

## Enabling Auto-Execution

To enable automatic execution of Copilot suggestions, modify `ExecuteWithCopilotCLIAsync()` in [ChaosCommandExecutor.cs](ChaosCommandExecutor.cs) to actually run the suggested commands.

⚠️ **Warning**: This will automatically modify your codebase based on donation-triggered chaos requests!

## Example Flow

1. Donor gives $10 → Tiltify webhook received
2. GitHub issue created: "Chaos Monkey: Rename a test to something ridiculous"
3. Background service detects issue after ~15-30 seconds
4. Service adds "processing" label and comment
5. Executor parses the task and runs it locally
6. Service adds "completed" label, posts results, closes issue
7. You see the chaos changes in your working directory!

## Logging

The service logs at various levels:
- `Information`: Task processing start/completion
- `Debug`: Polling attempts with no new issues
- `Error`: Failures during execution

Check your application logs to monitor the service activity.

## Stopping the Service

The background service runs as long as the application is running. To stop it:
- Stop the web application
- The service will gracefully shut down on cancellation

## Future Enhancements

- Git branch creation for each chaos task
- Automatic PR creation with chaos changes
- Rate limiting to prevent too many chaos tasks
- Whitelist/blacklist of allowed chaos operations
- Integration with VS Code extension for live feedback
