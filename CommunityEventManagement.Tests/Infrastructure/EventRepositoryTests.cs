using CommunityEventManagement.Domain.Entities;
using CommunityEventManagement.Infrastructure.Data;
using CommunityEventManagement.Infrastructure.Repositories;
using CommunityEventManagement.Tests.TestHelpers;
using Microsoft.EntityFrameworkCore;

namespace CommunityEventManagement.Tests.Infrastructure;

/// <summary>
/// These tests check the EventRepository against a real (in-memory SQLite) database. They prove
/// that my data access actually saves and reads data correctly, and that the database rules I set
/// up (like the unique index that stops duplicate registrations) really work.
/// Each test follows the Arrange / Act / Assert pattern.
/// </summary>
public class EventRepositoryTests : IDisposable
{
    private readonly TestDbContextFactory _factory;
    private readonly EventRepository _eventRepository;

    public EventRepositoryTests()
    {
        _factory = new TestDbContextFactory();
        _eventRepository = new EventRepository(_factory);
    }

    public void Dispose() => _factory.Dispose();

    [Fact]
    public async Task AddAsync_WithVenueAndActivity_SavesEventAndItsLinks()
    {
        // Arrange — put a venue and an activity in the database first.
        Guid guidVenueId;
        Guid guidActivityId;
        using (ApplicationDbContext context = _factory.CreateDbContext())
        {
            Venue venue = new Venue("Town Hall", "1 High Street", 100, true);
            WorkshopActivity activity = new WorkshopActivity("Art Class", 60, "Mr Bob", "Paint");
            context.Venues.Add(venue);
            context.Activities.Add(activity);
            await context.SaveChangesAsync();
            guidVenueId = venue.Id;
            guidActivityId = activity.Id;
        }

        Event newEvent = new Event("Spring Fair", DateTime.Today.AddDays(5),
            new TimeSpan(10, 0, 0), new TimeSpan(14, 0, 0), "A lovely fair", 100);

        // Act — save the event together with its venue and activity links.
        await _eventRepository.AddAsync(newEvent, new[] { guidVenueId }, new[] { guidActivityId });

        // Assert — read it back and check the links were created.
        Event? savedEvent = await _eventRepository.GetByIdAsync(newEvent.Id);
        Assert.NotNull(savedEvent);
        Assert.Equal("Spring Fair", savedEvent!.Name);
        Assert.Single(savedEvent.Venues);
        Assert.Single(savedEvent.Activities);
    }

    [Fact]
    public async Task GetByIdAsync_WhenIdDoesNotExist_ReturnsNull()
    {
        // Act — ask for an event using an Id that was never added.
        Event? result = await _eventRepository.GetByIdAsync(Guid.NewGuid());

        // Assert — nothing should come back.
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByDateAsync_ReturnsOnlyEventsOnThatDate()
    {
        // Arrange — two events on different dates.
        DateTime dtTarget = DateTime.Today.AddDays(10);
        await _eventRepository.AddAsync(
            new Event("On Target", dtTarget, new TimeSpan(9, 0, 0), new TimeSpan(10, 0, 0), "x", 50),
            Array.Empty<Guid>(), Array.Empty<Guid>());
        await _eventRepository.AddAsync(
            new Event("Different Day", dtTarget.AddDays(3), new TimeSpan(9, 0, 0), new TimeSpan(10, 0, 0), "y", 50),
            Array.Empty<Guid>(), Array.Empty<Guid>());

        // Act — filter by the target date.
        List<Event> results = await _eventRepository.GetByDateAsync(dtTarget);

        // Assert — only the matching event comes back.
        Assert.Single(results);
        Assert.Equal("On Target", results[0].Name);
    }

    [Fact]
    public async Task SearchAsync_FilterByVenue_ReturnsOnlyEventsAtThatVenue()
    {
        // Arrange — one venue, and two events where only one uses the venue.
        Guid guidVenueId;
        using (ApplicationDbContext context = _factory.CreateDbContext())
        {
            Venue venue = new Venue("Stadium", "Sports Road", 1000, false);
            context.Venues.Add(venue);
            await context.SaveChangesAsync();
            guidVenueId = venue.Id;
        }

        await _eventRepository.AddAsync(
            new Event("At Stadium", DateTime.Today.AddDays(2), new TimeSpan(9, 0, 0), new TimeSpan(11, 0, 0), "x", 50),
            new[] { guidVenueId }, Array.Empty<Guid>());
        await _eventRepository.AddAsync(
            new Event("No Venue", DateTime.Today.AddDays(2), new TimeSpan(9, 0, 0), new TimeSpan(11, 0, 0), "y", 50),
            Array.Empty<Guid>(), Array.Empty<Guid>());

        // Act — search by the venue id only.
        List<Event> results = await _eventRepository.SearchAsync(null, null, guidVenueId, null);

        // Assert — only the event held at that venue is returned.
        Assert.Single(results);
        Assert.Equal("At Stadium", results[0].Name);
    }

    [Fact]
    public async Task SearchAsync_FilterByActivityType_UsesTheTphDiscriminator()
    {
        // Arrange — a Workshop activity, and two events where only one includes it.
        Guid guidWorkshopId;
        using (ApplicationDbContext context = _factory.CreateDbContext())
        {
            WorkshopActivity workshop = new WorkshopActivity("Pottery", 90, "Jane", "Clay");
            context.Activities.Add(workshop);
            await context.SaveChangesAsync();
            guidWorkshopId = workshop.Id;
        }

        await _eventRepository.AddAsync(
            new Event("Has Workshop", DateTime.Today.AddDays(2), new TimeSpan(9, 0, 0), new TimeSpan(11, 0, 0), "x", 50),
            Array.Empty<Guid>(), new[] { guidWorkshopId });
        await _eventRepository.AddAsync(
            new Event("No Workshop", DateTime.Today.AddDays(2), new TimeSpan(9, 0, 0), new TimeSpan(11, 0, 0), "y", 50),
            Array.Empty<Guid>(), Array.Empty<Guid>());

        // Act — filter by the activity type, which reads the TPH discriminator column.
        List<Event> results = await _eventRepository.SearchAsync(null, null, null, "Workshop");

        // Assert — only the event that includes a workshop is returned.
        Assert.Single(results);
        Assert.Equal("Has Workshop", results[0].Name);
    }

    [Fact]
    public async Task DeleteAsync_RemovesTheEventAndCascadesToItsRegistrations()
    {
        // Arrange — an event with a participant registered for it.
        RegistrationRepository registrationRepository = new RegistrationRepository(_factory);
        Event newEvent = new Event("Doomed", DateTime.Today.AddDays(1), new TimeSpan(9, 0, 0), new TimeSpan(10, 0, 0), "x", 50);
        await _eventRepository.AddAsync(newEvent, Array.Empty<Guid>(), Array.Empty<Guid>());

        Guid guidParticipantId;
        using (ApplicationDbContext context = _factory.CreateDbContext())
        {
            Participant participant = new Participant("Sam", "Lee", "sam@b.com", "0700");
            context.Participants.Add(participant);
            await context.SaveChangesAsync();
            guidParticipantId = participant.Id;
        }
        await registrationRepository.AddAsync(new Registration(newEvent.Id, guidParticipantId, "Confirmed"));

        // Act — delete the event.
        await _eventRepository.DeleteAsync(newEvent.Id);

        // Assert — the event is gone, and its registration was cascade-deleted with it.
        using ApplicationDbContext check = _factory.CreateDbContext();
        Assert.Empty(check.Events);
        Assert.Empty(check.Registrations);
    }

    [Fact]
    public async Task Registration_WithDuplicateEventAndParticipant_ViolatesUniqueIndex()
    {
        // Arrange — one event and one participant in the database.
        RegistrationRepository registrationRepository = new RegistrationRepository(_factory);
        Guid guidEventId;
        Guid guidParticipantId;
        using (ApplicationDbContext context = _factory.CreateDbContext())
        {
            Event eventRow = new Event("Concert", DateTime.Today.AddDays(1), new TimeSpan(19, 0, 0), new TimeSpan(22, 0, 0), "music", 100);
            Participant participantRow = new Participant("Sam", "Lee", "sam@example.com", "0700");
            context.Events.Add(eventRow);
            context.Participants.Add(participantRow);
            await context.SaveChangesAsync();
            guidEventId = eventRow.Id;
            guidParticipantId = participantRow.Id;
        }

        // The first registration is fine.
        await registrationRepository.AddAsync(new Registration(guidEventId, guidParticipantId, "Confirmed"));

        // Act & Assert — a second registration for the same event and participant must be rejected
        // by the unique index, which EF surfaces as a DbUpdateException.
        await Assert.ThrowsAsync<DbUpdateException>(async () =>
            await registrationRepository.AddAsync(new Registration(guidEventId, guidParticipantId, "Confirmed")));
    }

    // ----- GetUpcomingAsync — filters out cancelled and past events -----

    [Fact]
    public async Task GetUpcomingAsync_ExcludesCancelledEvents()
    {
        // Arrange — one future active event and one future but cancelled event.
        Event activeEvent = new Event("Active", DateTime.Today.AddDays(3),
            new TimeSpan(9, 0, 0), new TimeSpan(10, 0, 0), "x", 50);
        Event cancelledEvent = new Event("Cancelled", DateTime.Today.AddDays(5),
            new TimeSpan(9, 0, 0), new TimeSpan(10, 0, 0), "y", 50);
        cancelledEvent.Cancel("Rain");

        await _eventRepository.AddAsync(activeEvent, Array.Empty<Guid>(), Array.Empty<Guid>());
        await _eventRepository.AddAsync(cancelledEvent, Array.Empty<Guid>(), Array.Empty<Guid>());

        // Act
        List<Event> results = await _eventRepository.GetUpcomingAsync();

        // Assert — only the non-cancelled event appears.
        Assert.Single(results);
        Assert.Equal("Active", results[0].Name);
    }

    [Fact]
    public async Task GetUpcomingAsync_ExcludesPastEvents()
    {
        // We cannot store a past date via the normal AddAsync flow without bypassing validation,
        // so we write directly to the context to simulate an event that has already passed.
        using (ApplicationDbContext ctx = _factory.CreateDbContext())
        {
            Event pastEvent = new Event("Past", DateTime.Today.AddDays(-1),
                new TimeSpan(9, 0, 0), new TimeSpan(10, 0, 0), "old", 50);
            ctx.Events.Add(pastEvent);
            await ctx.SaveChangesAsync();
        }

        List<Event> results = await _eventRepository.GetUpcomingAsync();

        Assert.Empty(results);
    }

    [Fact]
    public async Task GetUpcomingAsync_IncludesTodaysEvents()
    {
        // Boundary: an event happening today must appear in the upcoming list.
        await _eventRepository.AddAsync(
            new Event("Today", DateTime.Today, new TimeSpan(18, 0, 0), new TimeSpan(20, 0, 0), "tonight", 100),
            Array.Empty<Guid>(), Array.Empty<Guid>());

        List<Event> results = await _eventRepository.GetUpcomingAsync();

        Assert.Single(results);
        Assert.Equal("Today", results[0].Name);
    }

    // ----- GetAllAsync -----

    [Fact]
    public async Task GetAllAsync_ReturnsAllSavedEvents()
    {
        await _eventRepository.AddAsync(
            new Event("Alpha", DateTime.Today.AddDays(1), new TimeSpan(9, 0, 0), new TimeSpan(10, 0, 0), "a", 20),
            Array.Empty<Guid>(), Array.Empty<Guid>());
        await _eventRepository.AddAsync(
            new Event("Beta", DateTime.Today.AddDays(2), new TimeSpan(9, 0, 0), new TimeSpan(10, 0, 0), "b", 20),
            Array.Empty<Guid>(), Array.Empty<Guid>());

        List<Event> results = await _eventRepository.GetAllAsync();

        Assert.Equal(2, results.Count);
    }

    // ----- SearchAsync by search term -----

    [Fact]
    public async Task SearchAsync_BySearchTerm_ReturnsEventsWhoseNameMatches()
    {
        await _eventRepository.AddAsync(
            new Event("Summer Festival", DateTime.Today.AddDays(10), new TimeSpan(12, 0, 0), new TimeSpan(20, 0, 0), "fun", 500),
            Array.Empty<Guid>(), Array.Empty<Guid>());
        await _eventRepository.AddAsync(
            new Event("Winter Market", DateTime.Today.AddDays(14), new TimeSpan(10, 0, 0), new TimeSpan(18, 0, 0), "cold", 200),
            Array.Empty<Guid>(), Array.Empty<Guid>());

        List<Event> results = await _eventRepository.SearchAsync("Summer", null, null, null);

        Assert.Single(results);
        Assert.Equal("Summer Festival", results[0].Name);
    }

    [Fact]
    public async Task SearchAsync_WithNoFilters_ReturnsAllEvents()
    {
        await _eventRepository.AddAsync(
            new Event("One", DateTime.Today.AddDays(1), new TimeSpan(9, 0, 0), new TimeSpan(10, 0, 0), "x", 10),
            Array.Empty<Guid>(), Array.Empty<Guid>());
        await _eventRepository.AddAsync(
            new Event("Two", DateTime.Today.AddDays(2), new TimeSpan(9, 0, 0), new TimeSpan(10, 0, 0), "y", 10),
            Array.Empty<Guid>(), Array.Empty<Guid>());

        List<Event> results = await _eventRepository.SearchAsync(null, null, null, null);

        Assert.Equal(2, results.Count);
    }

    // ----- UpdateAsync -----

    [Fact]
    public async Task UpdateAsync_ChangesTheEventNameInTheDatabase()
    {
        // Arrange — save an event, then change its name via UpdateAsync.
        Event original = new Event("OldName", DateTime.Today.AddDays(5),
            new TimeSpan(9, 0, 0), new TimeSpan(10, 0, 0), "desc", 50);
        await _eventRepository.AddAsync(original, Array.Empty<Guid>(), Array.Empty<Guid>());

        Event updated = new Event("NewName", DateTime.Today.AddDays(5),
            new TimeSpan(9, 0, 0), new TimeSpan(10, 0, 0), "desc", 50);
        // Copy the Id so UpdateAsync knows which row to change.
        typeof(Event).GetProperty("Id")!.SetValue(updated, original.Id);

        // Act
        await _eventRepository.UpdateAsync(updated, Array.Empty<Guid>(), Array.Empty<Guid>());

        // Assert — read back and verify the name changed.
        Event? reloaded = await _eventRepository.GetByIdAsync(original.Id);
        Assert.Equal("NewName", reloaded!.Name);
    }

    // ----- SaveCancellationAsync -----

    [Fact]
    public async Task SaveCancellationAsync_PersistsIsCancelledAndReason()
    {
        Event evt = new Event("To Cancel", DateTime.Today.AddDays(3),
            new TimeSpan(9, 0, 0), new TimeSpan(10, 0, 0), "x", 50);
        await _eventRepository.AddAsync(evt, Array.Empty<Guid>(), Array.Empty<Guid>());

        evt.Cancel("Venue unavailable");
        await _eventRepository.SaveCancellationAsync(evt);

        Event? reloaded = await _eventRepository.GetByIdAsync(evt.Id);
        Assert.True(reloaded!.IsCancelled);
        Assert.Equal("Venue unavailable", reloaded.CancellationReason);
    }
}
