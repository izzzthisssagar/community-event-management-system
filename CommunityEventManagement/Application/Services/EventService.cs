using CommunityEventManagement.Domain.Entities;
using CommunityEventManagement.Domain.Exceptions;
using CommunityEventManagement.Domain.Interfaces;
using CommunityEventManagement.Models.ViewModels;

namespace CommunityEventManagement.Application.Services;

/// <summary>
/// IEventService is the contract for the event business logic. The Blazor pages depend on this
/// interface, not on the concrete class, which keeps the layers loosely coupled.
/// </summary>
public interface IEventService
{
    // ----- These three methods are an example of METHOD OVERLOADING. They all share the same
    // name (GetEventsAsync) but take different parameters, and the compiler picks the right one
    // based on the arguments I pass in. -----

    // Overload 1: get every event.
    Task<List<Event>> GetEventsAsync();

    // Overload 2: get the events on a specific date.
    Task<List<Event>> GetEventsAsync(DateTime dtDate);

    // Overload 3: get events that match a flexible set of search filters.
    Task<List<Event>> GetEventsAsync(string? sSearchTerm, DateTime? dtDate, Guid? guidVenueId, string? sActivityType);

    // Gets only upcoming, non-cancelled events for the public browse page.
    Task<List<Event>> GetUpcomingEventsAsync();

    // Gets one event by Id. Throws EventNotFoundException if it does not exist.
    Task<Event> GetEventByIdAsync(Guid guidId);

    // Creates a new event from the data the admin typed into the form.
    Task CreateEventAsync(EventViewModel vmEvent);

    // Updates an existing event from the form data.
    Task UpdateEventAsync(EventViewModel vmEvent);

    // Cancels an event (uses the ICancelable.Cancel method on the Event).
    Task CancelEventAsync(Guid guidId, string sReason);

    // Permanently deletes an event.
    Task DeleteEventAsync(Guid guidId);
}

/// <summary>
/// EventService contains the business logic for events. It sits between the Blazor pages and the
/// repository: the pages call the service, and the service calls the repository. Keeping the
/// logic here (and not in the pages) means the same rules are used everywhere.
/// </summary>
public class EventService : IEventService
{
    private readonly IEventRepository _erEventRepository;

    public EventService(IEventRepository erEventRepository)
    {
        _erEventRepository = erEventRepository;
    }

    // Overload 1 — simply hands back every event.
    public async Task<List<Event>> GetEventsAsync()
    {
        return await _erEventRepository.GetAllAsync();
    }

    // Overload 2 — events on a particular date.
    public async Task<List<Event>> GetEventsAsync(DateTime dtDate)
    {
        return await _erEventRepository.GetByDateAsync(dtDate);
    }

    // Overload 3 — the flexible search used by the debounced filter on the browse page.
    public async Task<List<Event>> GetEventsAsync(string? sSearchTerm, DateTime? dtDate, Guid? guidVenueId, string? sActivityType)
    {
        return await _erEventRepository.SearchAsync(sSearchTerm, dtDate, guidVenueId, sActivityType);
    }

    public async Task<List<Event>> GetUpcomingEventsAsync()
    {
        return await _erEventRepository.GetUpcomingAsync();
    }

    public async Task<Event> GetEventByIdAsync(Guid guidId)
    {
        Event? foundEvent = await _erEventRepository.GetByIdAsync(guidId);

        // If nothing came back, the id was wrong, so I throw my custom exception. The error
        // boundary will then show the user a friendly message instead of a crash.
        if (foundEvent is null)
        {
            throw new EventNotFoundException($"No event was found with Id '{guidId}'.");
        }

        return foundEvent;
    }

    public async Task CreateEventAsync(EventViewModel vmEvent)
    {
        // Build a real Event from the form data using the proper constructor.
        Event newEvent = new Event(
            vmEvent.Name,
            vmEvent.Date,
            vmEvent.StartTime,
            vmEvent.EndTime,
            vmEvent.Description,
            vmEvent.MaxCapacity);

        await _erEventRepository.AddAsync(newEvent, vmEvent.SelectedVenueIds, vmEvent.SelectedActivityIds);
    }

    public async Task UpdateEventAsync(EventViewModel vmEvent)
    {
        // An edit must have an Id. If it does not, something has gone wrong.
        if (vmEvent.Id is null)
        {
            throw new EventManagementException("Cannot update an event without an Id.");
        }

        // Load the existing event so I have its correct Id, then copy the new values onto it.
        Event existingEvent = await GetEventByIdAsync(vmEvent.Id.Value);
        existingEvent.Name = vmEvent.Name;
        existingEvent.Date = vmEvent.Date;
        existingEvent.StartTime = vmEvent.StartTime;
        existingEvent.EndTime = vmEvent.EndTime;
        existingEvent.Description = vmEvent.Description;
        existingEvent.MaxCapacity = vmEvent.MaxCapacity;

        await _erEventRepository.UpdateAsync(existingEvent, vmEvent.SelectedVenueIds, vmEvent.SelectedActivityIds);
    }

    public async Task CancelEventAsync(Guid guidId, string sReason)
    {
        // Load the event, then cancel it through its own Cancel() method (from ICancelable).
        Event eventToCancel = await GetEventByIdAsync(guidId);
        eventToCancel.Cancel(sReason);

        // Save the cancelled state to the database.
        await _erEventRepository.SaveCancellationAsync(eventToCancel);
    }

    public async Task DeleteEventAsync(Guid guidId)
    {
        await _erEventRepository.DeleteAsync(guidId);
    }
}
