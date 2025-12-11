Chaos Monkey for St. Jude Fundraiser – Technical Overview

📖 What is the Chaos Monkey?

The Chaos Monkey is a background agent designed to introduce controlled, humorous sabotage into the CodeMedic project during live streams. Inspired by Netflix’s Chaos Monkey, this version focuses on playful code mutations triggered by viewer donations. The goal is to entertain the audience, showcase debugging skills, and raise funds for St. Jude.

Key behaviors:

Random code mutations (loops → recursion, variable renames, logging spam).

Configuration sabotage (flip flags, change ports).

Unit test humor (rename tests, add goofy assertions).

Dependency chaos (downgrade packages, add unnecessary references).

🔌 Integration with Tiltify and GitHub

Tiltify → GitHub Workflow

Donation Event: A viewer donates via Tiltify.

Webhook Trigger: Tiltify sends a webhook to a custom handler.

Issue Creation: The handler creates a GitHub issue in the CodeMedic repo with chaos instructions.

GitHub Coding Agent: The agent picks up the issue, applies mutations, and opens a pull request.

Streamer Action: The PR is merged live, introducing chaos into the codebase.

Example Flow

$5 donation → Tiltify webhook → GitHub issue: “Chaos Monkey: Add silly log line to unit test.”

GitHub Coding Agent → Creates PR with mutation.

Streamer merges PR → Chaos visible in CodeMedic.

🧑‍💻 Code Requirements

1. Webhook Handler (Node.js or C#)

Receives Tiltify webhook payload.

Maps donation amount to chaos instruction.

Uses GitHub API to create issue with label chaos.

// Pseudocode for webhook handler
public async Task HandleTiltifyWebhook(Donation donation)
{
    string chaosInstruction = ChaosMapper.Map(donation.Amount);
    await GitHubApi.CreateIssue("codemedic-repo", chaosInstruction, labels: new[]{"chaos"});
}

2. Chaos Mapper

Maps donation tiers to chaos actions.

public static string Map(decimal amount)
{
    if (amount == 5) return "Add silly log line to unit test.";
    if (amount < 5) return "Simple chaos mutation.";
    return "General chaos mutation.";
}

3. GitHub Coding Agent Configuration

Configure agent with custom prompt: “Act as Chaos Monkey. Apply troll mutations to CodeMedic project.”

Agent listens for issues labeled chaos.

Agent generates code changes and opens PR.

4. Chaos Modules (Examples)

UnitTestChaos.cs → Adds silly logs and placeholder tests.

ConfigChaos.cs → Flips flags and ports.

CodeChaos.cs → Mutates methods with recursion, sleeps, renames.

DependencyChaos.cs → Alters NuGet references.

📅 Rollout Plan

Tomorrow: Configure webhook handler for Tiltify donations.

Weekend: Develop small C# application with chaos modules.

Next Week: Deploy to Azure Container Apps using Aspire.

Ongoing: Integrate with GitHub Coding Agent for autonomous chaos PRs.

🛠️ Technical Requirements for C# Chaos Monkey App

Build a lightweight C# application designed to run inside a container.

Use Aspire for building and deployment pipelines.

Target deployment on Azure Container Apps for scalability and ease of management.

Implement webhook listener to receive Tiltify donation events.

Map donation amounts to chaos instructions internally (no donation values exposed in code or documentation).

Use GitHub API to create issues with chaos instructions labeled chaos.

Keep the application small and focused to enable a 2-hour live stream build.

Include modular chaos components for easy extension and testing.

Ensure logging and error handling for observability during live streams.

🎯 Tiltify API and Webhook Integration

To get started with Tiltify API and webhooks:

Tiltify API GitHub repository: https://github.com/Tiltify/api

Tiltify webhook integration example (NodeCG bundle): https://github.com/daniellockard/nodecg-tiltify

Tiltify API client library example: https://github.com/daniellockard/tiltify-api-client

Getting Started

Register your application in the Tiltify dashboard to obtain client credentials.

Set up webhook endpoints in your C# application to receive donation event notifications.

Use the webhook payload to trigger chaos instructions and create GitHub issues.

Refer to the NodeCG example repository for guidance on configuring webhooks and handling real-time donation updates.

---