namespace CommunityEventManagement.Models.ViewModels;

/// <summary>
/// ActivityViewModel is the object the activity "create" and "edit" forms bind to. Because an
/// activity can be one of three different types (Workshop, Game or Talk), this one view model
/// holds the fields for ALL three types. The form shows the right fields depending on which
/// ActivityType the admin picks, and my service then builds the correct subclass from it.
/// </summary>
public class ActivityViewModel
{
    public Guid? Id { get; set; }

    // Which subclass to build: "Workshop", "Game" or "Talk".
    public string ActivityType { get; set; } = "Workshop";

    // Shared fields that every activity type has.
    public string Title { get; set; } = string.Empty;
    public int DurationMinutes { get; set; } = 60;

    // Workshop-only fields.
    public string InstructorName { get; set; } = string.Empty;
    public string MaterialsRequired { get; set; } = string.Empty;

    // Game-only fields.
    public int MinimumAge { get; set; } = 0;
    public bool EquipmentProvided { get; set; } = true;

    // Talk-only fields.
    public string SpeakerName { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
}
