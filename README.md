# CodeChaosMonkey
A tool that listens to Tiltify and messes with the code in my current project

## Queue System

The Chaos Monkey uses a JSON file-based queue system to track donation-triggered chaos requests. When a donation is received via the Tiltify webhook, it's added to a queue and processed automatically by the background service.

### Queue File Location

The queue file location can be configured in `appsettings.json`:

```json
"ChaosMonkey": {
  "QueueFilePath": "chaos-queue.json"
}
```

By default, the queue file is stored in the application's base directory as `chaos-queue.json`. This file is automatically created if it doesn't exist and is excluded from version control via `.gitignore`.

### Queue Monitoring

You can monitor the queue status by accessing the `/queue/status` endpoint, which returns:
- The number of pending items
- Details of each pending item (donation ID, donor name, amount, description, etc.)

### Queue Item States

Each queue item can be in one of four states:
- **Pending**: Waiting to be processed
- **Processing**: Currently being executed
- **Completed**: Successfully executed
- **Failed**: Execution failed
