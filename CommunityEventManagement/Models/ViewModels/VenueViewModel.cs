namespace CommunityEventManagement.Models.ViewModels;

/// <summary>
/// VenueViewModel is the object the venue "create" and "edit" forms bind to.
/// </summary>
public class VenueViewModel
{
    public Guid? Id { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public int Capacity { get; set; } = 100;
    public bool IsAccessible { get; set; } = true;
}
