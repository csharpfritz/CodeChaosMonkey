# Chaos Monkey Webhook API

This API provides endpoints to receive webhooks from Tiltify and create chaos requests in GitHub.

## Endpoints

### POST `/webhooks/tiltify`
Receives Tiltify donation webhooks and creates GitHub issues with chaos instructions.

**Request Body:**
```json
{
  "type": "donation",
  "data": {
    "id": "donation-id",
    "campaignId": "campaign-id",
    "amount": 25.00,
    "currency": "USD",
    "donorName": "John Doe",
    "donorComment": "Great stream!",
    "createdAt": "2025-12-10T10:00:00Z"
  }
}
```

**Response:**
```json
{
  "success": true,
  "message": "Successfully created chaos issue",
  "issueUrl": "https://github.com/owner/repo/issues/123"
}
```

### GET `/webhooks/tiltify/health`
Health check endpoint for the Tiltify webhook handler.

## Configuration

Set the following configuration values:

```json
{
  "GitHub": {
    "Owner": "your-github-username",
    "Repository": "your-repository-name",
    "Token": "your-github-personal-access-token"
  }
}
```

## Chaos Mapping

Donations are mapped to chaos instructions based on amount:

- $5: Add silly log line to unit test
- $20: Rename a test to something ridiculous  
- $50: Insert goofy placeholder test
- $100+: Introduce runtime chaos (exceptions, sleeps)
- Other amounts: General chaos mutation

## Running the Application

```bash
dotnet run
```

The API will be available at `https://localhost:5001` with Swagger documentation at `/swagger`.