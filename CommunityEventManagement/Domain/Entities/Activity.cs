namespace CommunityEventManagement.Domain.Entities;

/// <summary>
/// Activity is an ABSTRACT base class. The assignment scenario talks about different kinds of
/// activities like workshops, talks and games. Instead of using one flat class with a "Type"
/// string, I decided to use real inheritance and create a separate subclass for each type.
/// This class can never be created on its own (it is abstract) and it forces every subclass to
/// provide its own version of GetActivityDetails(). This is a strong demonstration of both
/// inheritance and polymorphism.
/// </summary>
public abstract class Activity : BaseEntity
{
    // Private backing field holding the events that include this activity. The relationship
    // between Event and Activity is many-to-many.
    private readonly List<Event> _events = new();

    // Protected parameterless constructor. It is protected (not private) because the subclasses
    // need to be able to call it, and EF Core also uses it when loading data.
    protected Activity() { }

    /// <summary>
    /// Protected constructor that the subclasses call (using : base(...)) to set the common
    /// activity values like the title and the duration.
    /// </summary>
    protected Activity(string sTitle, int iDurationMinutes)
    {
        Title = sTitle;
        DurationMinutes = iDurationMinutes;
    }

    public string Title { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }

    // The events that include this activity, exposed as read-only.
    public IReadOnlyCollection<Event> Events => _events.AsReadOnly();

    // This is an ABSTRACT method, which means it has no body here. Every subclass is forced to
    // write its own version. When I call GetActivityDetails() on an Activity reference, the
    // correct subclass version runs automatically. That is exactly what polymorphism means.
    public abstract string GetActivityDetails();
}

/// <summary>
/// WorkshopActivity is a workshop, for example a pottery or coding workshop. It adds its own
/// extra fields (the instructor and the materials) on top of the shared Activity fields.
/// </summary>
public class WorkshopActivity : Activity
{
    // Private parameterless constructor for EF Core.
    private WorkshopActivity() { }

    /// <summary>
    /// Public constructor. It first calls the base Activity constructor to set the shared
    /// values, then it sets the workshop-only values.
    /// </summary>
    public WorkshopActivity(string sTitle, int iDurationMinutes, string sInstructorName, string sMaterialsRequired)
        : base(sTitle, iDurationMinutes)
    {
        InstructorName = sInstructorName;
        MaterialsRequired = sMaterialsRequired;
    }

    public string InstructorName { get; set; } = string.Empty;
    public string MaterialsRequired { get; set; } = string.Empty;

    // My own version of the abstract method. It builds a workshop-specific description string.
    public override string GetActivityDetails()
    {
        return $"Workshop: {Title} ({DurationMinutes} min) — Instructor: {InstructorName}, Materials: {MaterialsRequired}";
    }
}

/// <summary>
/// GameActivity is a game, for example a community football match. It adds a minimum age and
/// a flag that says whether the equipment is provided.
/// </summary>
public class GameActivity : Activity
{
    // Private parameterless constructor for EF Core.
    private GameActivity() { }

    /// <summary>
    /// Public constructor that sets the shared values through base(...) and then the game values.
    /// </summary>
    public GameActivity(string sTitle, int iDurationMinutes, int iMinimumAge, bool bEquipmentProvided)
        : base(sTitle, iDurationMinutes)
    {
        MinimumAge = iMinimumAge;
        EquipmentProvided = bEquipmentProvided;
    }

    public int MinimumAge { get; set; }
    public bool EquipmentProvided { get; set; }

    // My own version of the abstract method, written for a game.
    public override string GetActivityDetails()
    {
        return $"Game: {Title} ({DurationMinutes} min) — Minimum Age: {MinimumAge}, Equipment Provided: {EquipmentProvided}";
    }
}

/// <summary>
/// TalkActivity is a talk or presentation, for example a guest speaker session. It adds the
/// speaker's name and the topic of the talk.
/// </summary>
public class TalkActivity : Activity
{
    // Private parameterless constructor for EF Core.
    private TalkActivity() { }

    /// <summary>
    /// Public constructor that sets the shared values through base(...) and then the talk values.
    /// </summary>
    public TalkActivity(string sTitle, int iDurationMinutes, string sSpeakerName, string sTopic)
        : base(sTitle, iDurationMinutes)
    {
        SpeakerName = sSpeakerName;
        Topic = sTopic;
    }

    public string SpeakerName { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;

    // My own version of the abstract method, written for a talk.
    public override string GetActivityDetails()
    {
        return $"Talk: {Title} ({DurationMinutes} min) — Speaker: {SpeakerName}, Topic: {Topic}";
    }
}
