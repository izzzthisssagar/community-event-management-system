namespace CommunityEventManagement.Domain.Entities;

/// <summary>
/// The Venue class represents a physical place where an event can be held, for example a
/// community hall or a sports field. It inherits from BaseEntity. An event can take place
/// at many venues and a venue can host many events, so this is a many-to-many relationship.
/// </summary>
public class Venue : BaseEntity
{
    // Private backing field holding the events that use this venue. Kept private for
    // encapsulation and named with the EF Core underscore convention.
    private readonly List<Event> _events = new();

    // Private parameterless constructor required by EF Core.
    private Venue() { }

    /// <summary>
    /// Public constructor that my code uses to create a venue with all of its details.
    /// </summary>
    public Venue(string sName, string sAddress, int iCapacity, bool bIsAccessible)
    {
        Name = sName;
        Address = sAddress;
        Capacity = iCapacity;
        IsAccessible = bIsAccessible;
    }

    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public int Capacity { get; set; }

    // IsAccessible is a true/false flag that records whether the venue has disabled access.
    // I included this to make the system a bit more realistic and useful.
    public bool IsAccessible { get; set; }

    // The events linked to this venue, exposed as read-only.
    public IReadOnlyCollection<Event> Events => _events.AsReadOnly();
}
