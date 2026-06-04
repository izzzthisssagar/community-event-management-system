namespace CommunityEventManagement.Domain.Entities;

/// <summary>
/// The Registration class is the link between a Participant and an Event. Because a participant
/// can join many events and an event can have many participants, this is a many-to-many
/// relationship. Registration is the "join" entity that sits in the middle, but it also carries
/// its own extra data (the registration date and the status), so it is a proper entity on its
/// own rather than just a hidden join table.
/// It inherits from BaseEntity and also implements ICancelable, because a registration can be
/// cancelled (for example if the participant pulls out). This is the second class that uses the
/// ICancelable interface, which is what makes the interface polymorphism real.
/// </summary>
public class Registration : BaseEntity, ICancelable
{
    // Private parameterless constructor required by EF Core.
    private Registration() { }

    /// <summary>
    /// Public constructor used when a participant registers for an event. It takes the two
    /// foreign keys and the status, and it automatically stamps the registration date.
    /// </summary>
    public Registration(Guid guidEventId, Guid guidParticipantId, string sStatus)
    {
        EventId = guidEventId;
        ParticipantId = guidParticipantId;
        Status = sStatus;
        RegistrationDate = DateTime.UtcNow;
    }

    // These are the two foreign keys. Their setters are private because once a registration is
    // created it should always stay linked to the same event and the same participant.
    public Guid EventId { get; private set; }
    public Guid ParticipantId { get; private set; }

    public DateTime RegistrationDate { get; private set; } = DateTime.UtcNow;
    public string Status { get; set; } = "Pending";

    // These two properties come from the ICancelable interface.
    public bool IsCancelled { get; private set; }
    public string? CancellationReason { get; private set; }

    // Navigation properties. EF Core uses these to load the related Event and Participant
    // objects. They are "null!" because EF will always fill them in when it loads the data.
    public Event Event { get; private set; } = null!;
    public Participant Participant { get; private set; } = null!;

    /// <summary>
    /// Cancels this registration. This is the Registration class's own version of the
    /// ICancelable.Cancel method. Notice it also sets the Status to "Cancelled", which is
    /// different from how the Event class cancels itself — same interface, different behaviour.
    /// </summary>
    public void Cancel(string sReason)
    {
        IsCancelled = true;
        CancellationReason = sReason;
        Status = "Cancelled";
        UpdatedAt = DateTime.UtcNow;
    }
}
