using CommunityEventManagement.Domain.Entities;

namespace CommunityEventManagement.Domain.Interfaces;

/// <summary>
/// IEventRepository is the contract (interface) for all database operations to do with events.
/// I use the Repository Pattern so that the rest of my application talks to this interface and
/// never touches Entity Framework directly. This keeps my data access in one place and means I
/// can swap or mock the implementation easily (which is very useful for unit testing).
/// I deliberately created a separate interface for each entity instead of one big generic
/// repository, because each entity needs its own specific queries (for example, only events
/// need to be searched by date or venue).
/// </summary>
public interface IEventRepository
{
    // Gets every event, including its linked venues, activities and registrations.
    Task<List<Event>> GetAllAsync();

    // Gets a single event by its Id, or null if it does not exist.
    Task<Event?> GetByIdAsync(Guid guidId);

    // Gets all events that fall on a particular date.
    Task<List<Event>> GetByDateAsync(DateTime dtDate);

    // Gets all upcoming (future, not cancelled) events for the public browse page.
    Task<List<Event>> GetUpcomingAsync();

    // The flexible search used by the debounced filter on the browse page. Every parameter is
    // optional, so the query is built up only from the filters the user actually chose.
    Task<List<Event>> SearchAsync(string? sSearchTerm, DateTime? dtDate, Guid? guidVenueId, string? sActivityType);

    // Adds a new event together with the venues and activities it should be linked to. The ids
    // are passed in so the repository can load those entities and create the join rows correctly.
    Task AddAsync(Event newEvent, IEnumerable<Guid> venueIds, IEnumerable<Guid> activityIds);

    // Updates an existing event and refreshes which venues and activities it is linked to.
    Task UpdateAsync(Event updatedEvent, IEnumerable<Guid> venueIds, IEnumerable<Guid> activityIds);

    // Saves the cancelled state of an event (after its Cancel() method has been called).
    Task SaveCancellationAsync(Event cancelledEvent);

    // Deletes the event with the given Id.
    Task DeleteAsync(Guid guidId);
}
