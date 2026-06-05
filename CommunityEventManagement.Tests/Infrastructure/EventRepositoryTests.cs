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
}
