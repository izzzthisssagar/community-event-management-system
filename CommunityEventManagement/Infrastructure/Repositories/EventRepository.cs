using CommunityEventManagement.Domain.Entities;
using CommunityEventManagement.Domain.Exceptions;
using CommunityEventManagement.Domain.Interfaces;
using CommunityEventManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CommunityEventManagement.Infrastructure.Repositories;

/// <summary>
/// EventRepository is the concrete implementation of IEventRepository. Because the class
/// implements the interface, anywhere that depends on IEventRepository can use this class — that
/// is polymorphism through an interface, and it is also what lets me swap in a mock during tests.
/// Every method follows the same safe pattern for Blazor Server: it asks the factory for a fresh,
/// short-lived DbContext, does its work, and the "using" keyword disposes the context right after.
/// </summary>
public class EventRepository : IEventRepository
{
    // I store the factory (not a DbContext). The "dcf" prefix follows my naming style for fields.
    private readonly IDbContextFactory<ApplicationDbContext> _dcfContextFactory;

    public EventRepository(IDbContextFactory<ApplicationDbContext> dcfContextFactory)
    {
        _dcfContextFactory = dcfContextFactory;
    }

    public async Task<List<Event>> GetAllAsync()
    {
        // Create a brand new context just for this operation.
        using ApplicationDbContext context = await _dcfContextFactory.CreateDbContextAsync();

        // Include pulls in the related data so the event is not empty. AsNoTracking is a small
        // performance win for read-only queries because EF does not need to track the results.
        return await context.Events
            .Include(e => e.Venues)
            .Include(e => e.Activities)
            .Include(e => e.Registrations)
                .ThenInclude(r => r.Participant)
            .AsNoTracking()
            .OrderBy(e => e.Date)
            .ToListAsync();
    }

    public async Task<Event?> GetByIdAsync(Guid guidId)
    {
        using ApplicationDbContext context = await _dcfContextFactory.CreateDbContextAsync();

        return await context.Events
            .Include(e => e.Venues)
            .Include(e => e.Activities)
            .Include(e => e.Registrations)
                .ThenInclude(r => r.Participant)
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == guidId);
    }

    public async Task<List<Event>> GetByDateAsync(DateTime dtDate)
    {
        using ApplicationDbContext context = await _dcfContextFactory.CreateDbContextAsync();

        // I compare only the Date part so the time of day does not matter. I fetch the matching
        // events first and then order them by start time in memory (with OrderBy after ToListAsync),
        // because ordering by a TimeSpan column is not supported by every database provider.
        List<Event> matchingEvents = await context.Events
            .Include(e => e.Venues)
            .Include(e => e.Activities)
            .AsNoTracking()
            .Where(e => e.Date.Date == dtDate.Date)
            .ToListAsync();

        return matchingEvents.OrderBy(e => e.StartTime).ToList();
    }

    public async Task<List<Event>> GetUpcomingAsync()
    {
        using ApplicationDbContext context = await _dcfContextFactory.CreateDbContextAsync();

        DateTime dtToday = DateTime.UtcNow.Date;

        // Only future events that have not been cancelled should appear on the public list.
        return await context.Events
            .Include(e => e.Venues)
            .Include(e => e.Activities)
            .AsNoTracking()
            .Where(e => e.Date >= dtToday && !e.IsCancelled)
            .OrderBy(e => e.Date)
            .ToListAsync();
    }

    public async Task<List<Event>> SearchAsync(string? sSearchTerm, DateTime? dtDate, Guid? guidVenueId, string? sActivityType)
    {
        using ApplicationDbContext context = await _dcfContextFactory.CreateDbContextAsync();

        // I start with the full set as an IQueryable. Nothing runs against the database yet — the
        // query is only built up in memory and is sent to MySQL as ONE efficient SQL statement at
        // the very end when I call ToListAsync. I only add each filter if the user actually chose
        // it, so the search is fully dynamic.
        IQueryable<Event> query = context.Events
            .Include(e => e.Venues)
            .Include(e => e.Activities)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(sSearchTerm))
        {
            query = query.Where(e => e.Name.Contains(sSearchTerm) || e.Description.Contains(sSearchTerm));
        }

        if (dtDate.HasValue)
        {
            query = query.Where(e => e.Date.Date == dtDate.Value.Date);
        }

        if (guidVenueId.HasValue)
        {
            query = query.Where(e => e.Venues.Any(v => v.Id == guidVenueId.Value));
        }

        if (!string.IsNullOrWhiteSpace(sActivityType))
        {
            // "ActivityType" is the TPH discriminator column. It is not a normal C# property, so I
            // read it with EF.Property to filter events by the type of activity they include.
            query = query.Where(e => e.Activities.Any(a => EF.Property<string>(a, "ActivityType") == sActivityType));
        }

        return await query.OrderBy(e => e.Date).ToListAsync();
    }

    public async Task AddAsync(Event newEvent, IEnumerable<Guid> venueIds, IEnumerable<Guid> activityIds)
    {
        using ApplicationDbContext context = await _dcfContextFactory.CreateDbContextAsync();

        // I load the chosen venues and activities INSIDE this context so they are tracked. That
        // way EF knows they already exist and only creates the join rows, instead of trying to
        // insert duplicate venues or activities.
        List<Guid> listVenueIds = venueIds.ToList();
        List<Guid> listActivityIds = activityIds.ToList();

        List<Venue> selectedVenues = await context.Venues.Where(v => listVenueIds.Contains(v.Id)).ToListAsync();
        List<Activity> selectedActivities = await context.Activities.Where(a => listActivityIds.Contains(a.Id)).ToListAsync();

        // I link them through my own domain methods, which keeps the rules inside the entity.
        foreach (Venue venue in selectedVenues)
        {
            newEvent.AddVenue(venue);
        }
        foreach (Activity activity in selectedActivities)
        {
            newEvent.AddActivity(activity);
        }

        context.Events.Add(newEvent);
        await context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Event updatedEvent, IEnumerable<Guid> venueIds, IEnumerable<Guid> activityIds)
    {
        using ApplicationDbContext context = await _dcfContextFactory.CreateDbContextAsync();

        // I load the real, tracked event (with its current venues and activities) from the
        // database. I then copy the new values onto it. This is the safe way to update when using
        // the factory pattern, because the object the user edited came from a different context.
        Event? existingEvent = await context.Events
            .Include(e => e.Venues)
            .Include(e => e.Activities)
            .FirstOrDefaultAsync(e => e.Id == updatedEvent.Id);

        if (existingEvent is null)
        {
            throw new EventNotFoundException($"Could not update because event with Id '{updatedEvent.Id}' was not found.");
        }

        // Copy across the simple values.
        existingEvent.Name = updatedEvent.Name;
        existingEvent.Date = updatedEvent.Date;
        existingEvent.StartTime = updatedEvent.StartTime;
        existingEvent.EndTime = updatedEvent.EndTime;
        existingEvent.Description = updatedEvent.Description;
        existingEvent.MaxCapacity = updatedEvent.MaxCapacity;
        existingEvent.UpdatedAt = DateTime.UtcNow;

        // Reset the venue links: remove all current ones, then add the newly chosen ones.
        foreach (Venue venue in existingEvent.Venues.ToList())
        {
            existingEvent.RemoveVenue(venue);
        }
        List<Guid> listVenueIds = venueIds.ToList();
        List<Venue> selectedVenues = await context.Venues.Where(v => listVenueIds.Contains(v.Id)).ToListAsync();
        foreach (Venue venue in selectedVenues)
        {
            existingEvent.AddVenue(venue);
        }

        // Reset the activity links the same way.
        foreach (Activity activity in existingEvent.Activities.ToList())
        {
            existingEvent.RemoveActivity(activity);
        }
        List<Guid> listActivityIds = activityIds.ToList();
        List<Activity> selectedActivities = await context.Activities.Where(a => listActivityIds.Contains(a.Id)).ToListAsync();
        foreach (Activity activity in selectedActivities)
        {
            existingEvent.AddActivity(activity);
        }

        await context.SaveChangesAsync();
    }

    public async Task SaveCancellationAsync(Event cancelledEvent)
    {
        using ApplicationDbContext context = await _dcfContextFactory.CreateDbContextAsync();

        // Load the real tracked event and cancel it through its own domain method, then save.
        Event? existingEvent = await context.Events.FirstOrDefaultAsync(e => e.Id == cancelledEvent.Id);
        if (existingEvent is null)
        {
            throw new EventNotFoundException($"Could not cancel because event with Id '{cancelledEvent.Id}' was not found.");
        }

        existingEvent.Cancel(cancelledEvent.CancellationReason ?? "Cancelled by the administrator.");
        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid guidId)
    {
        using ApplicationDbContext context = await _dcfContextFactory.CreateDbContextAsync();

        // Find the event first. If it exists, remove it; the cascade rules remove its join rows.
        Event? eventToDelete = await context.Events.FirstOrDefaultAsync(e => e.Id == guidId);
        if (eventToDelete is not null)
        {
            context.Events.Remove(eventToDelete);
            await context.SaveChangesAsync();
        }
    }
}
