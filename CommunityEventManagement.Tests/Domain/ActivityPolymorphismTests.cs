using CommunityEventManagement.Domain.Entities;

namespace CommunityEventManagement.Tests.Domain;

/// <summary>
/// These tests prove that the Activity inheritance hierarchy uses polymorphism correctly. The most
/// important test treats all three subclasses as the base Activity type and calls the same
/// GetActivityDetails() method on each — the correct subclass version runs automatically, which is
/// exactly what polymorphism through inheritance means.
/// </summary>
public class ActivityPolymorphismTests
{
    [Fact]
    public void WorkshopActivity_GetActivityDetails_IncludesTheInstructor()
    {
        Activity activity = new WorkshopActivity("Pottery", 90, "Jane Smith", "Clay");

        string details = activity.GetActivityDetails();

        Assert.Contains("Workshop", details);
        Assert.Contains("Jane Smith", details);
    }

    [Fact]
    public void GameActivity_GetActivityDetails_IncludesTheMinimumAge()
    {
        Activity activity = new GameActivity("Football", 60, 12, true);

        string details = activity.GetActivityDetails();

        Assert.Contains("Game", details);
        Assert.Contains("12", details);
    }

    [Fact]
    public void TalkActivity_GetActivityDetails_IncludesTheSpeaker()
    {
        Activity activity = new TalkActivity("Climate", 45, "Dr Green", "Sustainability");

        string details = activity.GetActivityDetails();

        Assert.Contains("Talk", details);
        Assert.Contains("Dr Green", details);
    }

    [Fact]
    public void GetActivityDetails_OnBaseTypeReferences_DispatchesToEachSubclass()
    {
        // I hold all three as the base Activity type and call the same method on each. Polymorphism
        // means each call runs the correct overridden version without me checking the type.
        List<Activity> activities = new List<Activity>
        {
            new WorkshopActivity("Pottery", 90, "Jane", "Clay"),
            new GameActivity("Football", 60, 12, true),
            new TalkActivity("Climate", 45, "Dr Green", "Sustainability")
        };

        List<string> allDetails = activities.Select(a => a.GetActivityDetails()).ToList();

        Assert.StartsWith("Workshop", allDetails[0]);
        Assert.StartsWith("Game", allDetails[1]);
        Assert.StartsWith("Talk", allDetails[2]);
    }
}
