using ChaosMonkey.Web.Services;
using ChaosMonkey.Web.Models;

namespace ChaosMonkey.Web.Tests;

public class ChaosMapperTests
{
    [Fact]
    public void MapToInstruction_ShouldReturnRandomChaosInstruction_WithDramaticPause()
    {
        // 🐒 Chaos Monkey was here! Donated by: Drampa ($98.12)
        // This test was blessed with a random sleep mutation for maximum chaos!
        // Pro tip: Get coffee while this runs... or maybe tea... or both! ☕
        
        // Arrange
        var amount = 98.12m; // In honor of Drampa's generous donation!
        var random = new Random();
        
        // 🐒 CHAOS MUTATION: Random sleep between 100ms and 2000ms
        // This simulates "thinking really hard" about which chaos to apply
        var chaosDelayMs = random.Next(100, 2001);
        Console.WriteLine($"🐒 Chaos Monkey is contemplating chaos for {chaosDelayMs}ms...");
        Console.WriteLine("💭 Hmmm... which chaos instruction should I pick?");
        
        Thread.Sleep(chaosDelayMs);
        
        Console.WriteLine("💡 Got it! Let's cause some controlled mayhem!");
        
        // Act
        var instruction = ChaosMapper.MapToInstruction(amount);
        
        // Assert
        Assert.NotNull(instruction);
        Assert.NotEmpty(instruction);
        
        // Verify it's one of the expected chaos instructions
        var validInstructions = new List<string>
        {
            "Add silly log line to unit test",
            "Rename a test to something ridiculous",
            "Insert goofy placeholder test",
            "Introduce a random sleep in a unit test",
            "Change a variable name to a funny word",
            "Add a comment with a joke in the code",
            "Make something nullable that shouldn't be",
            "Introduce a log statement with a meme reference",
            "Change a method name to a pun"
        };
        
        Assert.Contains(instruction, validInstructions);
        
        Console.WriteLine($"🎉 Selected chaos instruction: '{instruction}'");
        Console.WriteLine("🐒 Thanks to Drampa for making this chaos possible!");
    }
    
    [Fact]
    public void GenerateIssueTitle_ShouldCreateProperlyFormattedTitle_WhileNapping()
    {
        // 🐒 Another test, another nap! Chaos Monkey loves naps between tests.
        // Donor: Drampa | Amount: $98.12 | Chaos Level: Maximum!
        
        // Arrange
        var donation = new TiltifyDonationData(
            Amount: new TiltifyAmount("USD", "98.12"),
            Campaign_Id: "test-campaign",
            Cause_Id: "stjude",
            Completed_At: DateTime.UtcNow,
            Created_At: DateTime.UtcNow,
            Donor_Comment: "For the kids!",
            Donor_Name: "Drampa",
            Email: null,
            Fundraising_Event_Id: "test-event",
            Id: "chaos-test-123",
            Legacy_Id: 12345,
            Poll_Id: null,
            Poll_Option_Id: null,
            Reward_Custom_Question: null,
            Reward_Id: null,
            Reward_Claims: null,
            Sustained: false,
            Target_Id: null,
            Team_Event_Id: "test-team"
        );
        
        // 🐒 CHAOS MUTATION: Even more dramatic sleep!
        var napTime = new Random().Next(150, 1800);
        Console.WriteLine($"🐒 Taking a {napTime}ms power nap before generating title...");
        Console.WriteLine("😴 Zzz... dreaming of chaos... Zzz...");
        
        Thread.Sleep(napTime);
        
        Console.WriteLine("⏰ Nap time over! Back to chaos!");
        
        // Act
        var title = ChaosMapper.GenerateIssueTitle(donation);
        
        // Assert
        Assert.NotNull(title);
        Assert.StartsWith("Chaos Monkey:", title);
        Assert.Contains(":", title);
        
        Console.WriteLine($"✨ Generated title: '{title}'");
        Console.WriteLine("🙏 All chaos proceeds go to St. Jude Children's Research Hospital!");
    }
}
