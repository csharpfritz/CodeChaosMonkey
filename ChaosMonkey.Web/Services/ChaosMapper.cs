using ChaosMonkey.Web.Models;

namespace ChaosMonkey.Web.Services;

public class ChaosMapper
{
    public static string MapToInstruction(decimal amount)
    {
        return amount switch
        {
            5m => "Add silly log line to unit test",
            20m => "Rename a test to something ridiculous",
            50m => "Insert goofy placeholder test",
            >= 100m => "Introduce runtime chaos (exceptions, sleeps)",
            _ => "General chaos mutation"
        };
    }

    public static string GenerateIssueTitle(TiltifyDonationData donation)
    {
        var amount = decimal.Parse(donation.Amount.Value);
        var instruction = MapToInstruction(amount);
        return $"Chaos Monkey: {instruction}";
    }

    public static string GenerateIssueBody(TiltifyDonationData donation)
    {
        var amount = decimal.Parse(donation.Amount.Value);
        var instruction = MapToInstruction(amount);
        
        var body = $"""
            ## Chaos Monkey Request 🐒

            **Chaos Level**: {donation.Amount.Currency} ${amount:F2}
            **Requested by**: {donation.Donor_Name}
            **Instruction**: {instruction}
            **Donation ID**: {donation.Id}

            {(string.IsNullOrWhiteSpace(donation.Donor_Comment) ? "" : $"**Donor Message**: {donation.Donor_Comment}\n")}
            ---
            
            This chaos request was automatically generated from a donation event.
            Please apply the requested chaos mutation to the codebase.
            """;

        return body;
    }
}