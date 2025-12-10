# Test Tiltify Webhook Integration

## Testing the Webhook Endpoint

You can test the webhook endpoint using curl or any HTTP client:

```bash
# Test the health endpoint
curl -X GET https://localhost:5001/webhooks/tiltify/health

# Test the webhook endpoint with sample data
curl -X POST https://localhost:5120/webhooks/tiltify \
  -H "Content-Type: application/json" \
  -d '{
    "data": {
      "amount": {
        "currency": "USD",
        "value": "25.00"
      },
      "campaign_id": "test-campaign-456",
      "cause_id": "6e298b78-4082-4800-b2f9-7c00dd058134",
      "completed_at": null,
      "created_at": "2025-12-10T10:00:00Z",
      "donor_comment": "Testing the chaos monkey!",
      "donor_name": "Test Donor",
      "email": null,
      "fundraising_event_id": "d469b7dd-50c3-4224-b047-64d0a2884b7d",
      "id": "test-donation-123",
      "legacy_id": 0,
      "poll_id": null,
      "poll_option_id": null,
      "reward_custom_question": null,
      "reward_id": null,
      "reward_claims": null,
      "sustained": false,
      "target_id": null,
      "team_event_id": "37410c31-9f3a-4576-90dc-ac902bd4cbc3"
    },
    "meta": {
      "attempted_at": "2025-12-10T10:01:00Z",
      "event_type": "private:direct:donation_updated",
      "generated_at": "2025-12-10T10:00:30Z",
      "id": "530ddffa-dc8d-4661-a998-d17e85f726d8",
      "subscription_source_id": "00000000-0000-0000-0000-000000000000",
      "subscription_source_type": "test"
    }
  }'
```

## PowerShell Test Script

```powershell
# Test with PowerShell
$body = @{
    data = @{
        amount = @{
            currency = "USD"
            value = "25.00"
        }
        campaign_id = "test-campaign-456"
        cause_id = "6e298b78-4082-4800-b2f9-7c00dd058134"
        completed_at = $null
        created_at = "2025-12-10T10:00:00Z"
        donor_comment = "Testing the chaos monkey!"
        donor_name = "Test Donor"
        email = $null
        fundraising_event_id = "d469b7dd-50c3-4224-b047-64d0a2884b7d"
        id = "test-donation-123"
        legacy_id = 0
        poll_id = $null
        poll_option_id = $null
        reward_custom_question = $null
        reward_id = $null
        reward_claims = $null
        sustained = $false
        target_id = $null
        team_event_id = "37410c31-9f3a-4576-90dc-ac902bd4cbc3"
    }
    meta = @{
        attempted_at = "2025-12-10T10:01:00Z"
        event_type = "private:direct:donation_updated"
        generated_at = "2025-12-10T10:00:30Z"
        id = "530ddffa-dc8d-4661-a998-d17e85f726d8"
        subscription_source_id = "00000000-0000-0000-0000-000000000000"
        subscription_source_type = "test"
    }
} | ConvertTo-Json -Depth 4

Invoke-RestMethod -Uri "http://localhost:5120/webhooks/tiltify" -Method POST -Body $body -ContentType "application/json"
```

## Setup Requirements

1. **GitHub Token**: You need to create a GitHub Personal Access Token with `repo` permissions
2. **Configuration**: Update the `GitHub:Token` value in `appsettings.Development.json`
3. **Repository**: Make sure the `GitHub:Owner` and `GitHub:Repository` settings match your target repository

## Expected Workflow

1. Tiltify sends a webhook when someone donates
2. The webhook endpoint receives the donation data
3. The system creates a GitHub issue with chaos instructions
4. GitHub Coding Agent (or manual process) picks up the issue and creates a PR
5. The chaos is introduced to the codebase during the live stream