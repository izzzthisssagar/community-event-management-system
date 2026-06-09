namespace CommunityEventManagement.Models.ViewModels;

/// <summary>
/// RegistrationViewModel is used when registering a participant for an event. It simply carries
/// the two ids the registration needs: which event and which participant.
/// </summary>
public class RegistrationViewModel
{
    public Guid EventId { get; set; }
    public Guid ParticipantId { get; set; }
}
