namespace ChaosMonkey.Web.Models;

public record TiltifyWebhookPayload(
    TiltifyDonationData Data,
    TiltifyWebhookMeta Meta
);

public record TiltifyDonationData(
    TiltifyAmount Amount,
    string Campaign_Id,
    string Cause_Id,
    DateTime? Completed_At,
    DateTime Created_At,
    string? Donor_Comment,
    string Donor_Name,
    string? Email,
    string Fundraising_Event_Id,
    string Id,
    int Legacy_Id,
    string? Poll_Id,
    string? Poll_Option_Id,
    string? Reward_Custom_Question,
    string? Reward_Id,
    TiltifyRewardClaim[]? Reward_Claims,
    bool Sustained,
    string? Target_Id,
    string Team_Event_Id
);

public record TiltifyAmount(
    string Currency,
    string Value
);

public record TiltifyRewardClaim(
    string Id,
    string Reward_Id,
    int Quantity
);

public record TiltifyWebhookMeta(
    DateTime Attempted_At,
    string Event_Type,
    DateTime Generated_At,
    string Id,
    string Subscription_Source_Id,
    string Subscription_Source_Type
);

public record WebhookResponse(
    bool Success,
    string Message,
    string? IssueUrl = null
);