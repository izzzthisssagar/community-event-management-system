namespace CommunityEventManagement.Models.ViewModels;

/// <summary>
/// EventViewModel is the object my Blazor "create" and "edit" forms bind to. I use a separate
/// view model instead of binding the form straight to the Event entity. There are two good
/// reasons for this: the entity has private setters and a special constructor that a form can not
/// use, and the form needs extra helper fields (like the lists of selected venue and activity
/// ids) that are not really part of the Event itself. FluentValidation then validates this view
/// model before I ever build a real Event from it.
/// </summary>
public class EventViewModel
{
    // Null when creating a brand new event, set when editing an existing one.
    public Guid? Id { get; set; }

    public string Name { get; set; } = string.Empty;
    public DateTime Date { get; set; } = DateTime.Today.AddDays(7);
    public TimeSpan StartTime { get; set; } = new TimeSpan(10, 0, 0);
    public TimeSpan EndTime { get; set; } = new TimeSpan(12, 0, 0);
    public string Description { get; set; } = string.Empty;
    public int MaxCapacity { get; set; } = 50;

    // The venues and activities the admin ticked for this event.
    public List<Guid> SelectedVenueIds { get; set; } = new();
    public List<Guid> SelectedActivityIds { get; set; } = new();
}
