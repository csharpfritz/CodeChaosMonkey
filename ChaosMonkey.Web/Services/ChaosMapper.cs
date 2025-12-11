using ChaosMonkey.Web.Models;

namespace ChaosMonkey.Web.Services;

public class ChaosMapper
{
    static readonly List<string> instructions = new List<string> {
        "Rename a test to something ridiculous",
        "Insert goofy placeholder test",
        "Introduce a random sleep in a unit test",
        "Change a variable name to a funny word",
        "Make something nullable that shouldn't be",
        "Change a method name to a pun",
    };

    static readonly List<string> SimpleInstructions = new List<string> {
        "Add silly log line to unit test",
        "Add a comment with a joke in the code",
        "Introduce a log statement with a meme reference",
    };

    private static readonly DateTime FirstDate = new DateTime(2025, 12, 10);

    public static string MapToInstruction(decimal amount)
    {

        var random = new Random((int)(DateTime.Now - FirstDate).TotalSeconds);

        if (amount < 5)
        {
            return SimpleInstructions[random.Next(SimpleInstructions.Count)];
        }

        return instructions[random.Next(instructions.Count)];

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
        
        var chaosType = amount >= 5 ? "Complex" : "Simple";
        
        var body = $"""
            ## Chaos Monkey Request 🐒

            **Chaos Type:** {chaosType}
            **Chaos Level**: {donation.Amount.Currency} ${amount:F2}
            **Requested by**: {donation.Donor_Name}
            **Description:** {instruction}
            **Donation ID**: {donation.Id}

            ---
            
            This chaos request was automatically generated from a donation event.
            The Chaos Executor service will process this task automatically.
            """;

        return body;
    }
}