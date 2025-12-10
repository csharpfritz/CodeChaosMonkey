using ChaosMonkey.Web.Models;

namespace ChaosMonkey.Web.Services;

public class ChaosMapper
{
    public static string MapToInstruction(decimal amount)
    {

			var random = new Random();
			var instructions = new List<string> {
				"Add silly log line to unit test",
				"Rename a test to something ridiculous",
				"Insert goofy placeholder test",
				"Introduce a random sleep in a unit test",
				"Change a variable name to a funny word",
				"Add a comment with a joke in the code",
				"Make something nullable that shouldn't be",
				"Introduce a log statement with a meme reference",
				"Change a method name to a pun",
			};

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
        
        var body = $"""
            ## Chaos Monkey Request 🐒

            **Chaos Level**: {donation.Amount.Currency} ${amount:F2}
            **Requested by**: {donation.Donor_Name}
            **Instruction**: {instruction}
            **Donation ID**: {donation.Id}

            ---
            
            This chaos request was automatically generated from a donation event.
            Please apply the requested chaos mutation to the codebase.
            """;

        return body;
    }
}