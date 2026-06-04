namespace CommunityEventManagement.Models.ViewModels;

/// <summary>
/// ParticipantViewModel is the object the participant "create" and "edit" forms bind to.
/// </summary>
public class ParticipantViewModel
{
    public Guid? Id { get; set; }

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
}
