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

    // ----- Each subclass stores and surfaces its own specific data correctly -----

    [Fact]
    public void WorkshopActivity_MaterialsRequired_IsIncludedInDetails()
    {
        Activity activity = new WorkshopActivity("Pottery", 90, "Jane Smith", "Clay and tools");

        string details = activity.GetActivityDetails();

        Assert.Contains("Clay and tools", details);
    }

    [Fact]
    public void GameActivity_WhenEquipmentIsProvided_DetailsReflectTrue()
    {
        Activity activity = new GameActivity("Football", 60, 12, true);

        string details = activity.GetActivityDetails();

        // The details string must mention that equipment is provided.
        Assert.Contains("True", details, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void GameActivity_WhenEquipmentIsNotProvided_DetailsReflectFalse()
    {
        Activity activity = new GameActivity("Chess", 60, 8, false);

        string details = activity.GetActivityDetails();

        Assert.Contains("False", details, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TalkActivity_TopicIsIncludedInDetails()
    {
        Activity activity = new TalkActivity("Climate Change", 45, "Dr Green", "Carbon neutrality");

        string details = activity.GetActivityDetails();

        Assert.Contains("Carbon neutrality", details);
    }

    [Fact]
    public void AllActivitySubclasses_DurationIsStoredOnBaseClass()
    {
        // DurationMinutes is defined on the base Activity class; all subclasses must inherit it.
        Activity workshop = new WorkshopActivity("Art", 90, "Tutor", "Paint");
        Activity game = new GameActivity("Tag", 30, 6, true);
        Activity talk = new TalkActivity("History", 60, "Prof", "Wars");

        Assert.Equal(90, workshop.DurationMinutes);
        Assert.Equal(30, game.DurationMinutes);
        Assert.Equal(60, talk.DurationMinutes);
    }

    [Fact]
    public void ActivitySubclasses_AreAllAssignableToBaseActivityType()
    {
        // This confirms the inheritance hierarchy is correctly set up.
        Assert.IsAssignableFrom<Activity>(new WorkshopActivity("W", 60, "T", "M"));
        Assert.IsAssignableFrom<Activity>(new GameActivity("G", 60, 10, true));
        Assert.IsAssignableFrom<Activity>(new TalkActivity("T", 45, "S", "Top"));
    }
}
