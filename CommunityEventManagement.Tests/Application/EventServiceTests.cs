using CommunityEventManagement.Application.Services;
using CommunityEventManagement.Domain.Entities;
using CommunityEventManagement.Domain.Exceptions;
using CommunityEventManagement.Domain.Interfaces;
using CommunityEventManagement.Models.ViewModels;
using Moq;

namespace CommunityEventManagement.Tests.Application;

/// <summary>
/// These tests check the EventService business logic using a mocked repository.
/// </summary>
public class EventServiceTests
{
    private readonly Mock<IEventRepository> _mockEventRepository = new();
    private readonly EventService _eventService;

    public EventServiceTests()
    {
        _eventService = new EventService(_mockEventRepository.Object);
    }

    [Fact]
    public async Task CancelEventAsync_MarksTheEventAsCancelledWithTheReason()
    {
        // Arrange — a normal, not-yet-cancelled event.
        Event targetEvent = new Event("Fun Run", DateTime.Today.AddDays(7), new TimeSpan(9, 0, 0), new TimeSpan(12, 0, 0), "run", 200);
        _mockEventRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(targetEvent);
        _mockEventRepository.Setup(r => r.SaveCancellationAsync(It.IsAny<Event>())).Returns(Task.CompletedTask);

        // Act — cancel it through the service, which uses the ICancelable.Cancel method.
        await _eventService.CancelEventAsync(targetEvent.Id, "Bad weather");

        // Assert — the event now reports itself as cancelled, with my reason, and was saved once.
        Assert.True(targetEvent.IsCancelled);
        Assert.Equal("Bad weather", targetEvent.CancellationReason);
        _mockEventRepository.Verify(r => r.SaveCancellationAsync(targetEvent), Times.Once);
    }

    [Fact]
    public async Task GetEventsAsync_NoArguments_ReturnsEveryEvent()
    {
        // Arrange — the repository has two events.
        List<Event> storedEvents = new()
        {
            new Event("One", DateTime.Today.AddDays(1), new TimeSpan(9, 0, 0), new TimeSpan(10, 0, 0), "a", 10),
            new Event("Two", DateTime.Today.AddDays(2), new TimeSpan(9, 0, 0), new TimeSpan(10, 0, 0), "b", 10)
        };
        _mockEventRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(storedEvents);

        // Act — call the no-argument overload of GetEventsAsync.
        List<Event> results = await _eventService.GetEventsAsync();

        // Assert
        Assert.Equal(2, results.Count);
    }

    [Fact]
    public async Task GetEventByIdAsync_WhenTheEventDoesNotExist_ThrowsEventNotFoundException()
    {
        // Arrange — the repository finds nothing.
        _mockEventRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Event?)null);

        // Act & Assert — the service turns the missing event into my custom exception.
        await Assert.ThrowsAsync<EventNotFoundException>(async () =>
            await _eventService.GetEventByIdAsync(Guid.NewGuid()));
    }

    // ----- Method overloading: all three GetEventsAsync signatures must route correctly. -----

    [Fact]
    public async Task GetEventsAsync_ByDate_CallsGetByDateAsync()
    {
        // This is the SECOND overload. It should delegate to GetByDateAsync, not GetAllAsync.
        DateTime dtTarget = DateTime.Today.AddDays(5);
        _mockEventRepository.Setup(r => r.GetByDateAsync(dtTarget)).ReturnsAsync(new List<Event>());

        await _eventService.GetEventsAsync(dtTarget);

        _mockEventRepository.Verify(r => r.GetByDateAsync(dtTarget), Times.Once);
        _mockEventRepository.Verify(r => r.GetAllAsync(), Times.Never);
    }

    [Fact]
    public async Task GetEventsAsync_WithFilters_CallsSearchAsync()
    {
        // This is the THIRD overload. It should delegate to SearchAsync with all four parameters.
        _mockEventRepository
            .Setup(r => r.SearchAsync(It.IsAny<string?>(), It.IsAny<DateTime?>(), It.IsAny<Guid?>(), It.IsAny<string?>()))
            .ReturnsAsync(new List<Event>());

        await _eventService.GetEventsAsync("festival", null, null, "Workshop");

        _mockEventRepository.Verify(
            r => r.SearchAsync("festival", null, null, "Workshop"), Times.Once);
    }

    [Fact]
    public async Task GetUpcomingEventsAsync_DelegatesToRepository()
    {
        _mockEventRepository.Setup(r => r.GetUpcomingAsync()).ReturnsAsync(new List<Event>());

        await _eventService.GetUpcomingEventsAsync();

        _mockEventRepository.Verify(r => r.GetUpcomingAsync(), Times.Once);
    }

    // ----- CreateEventAsync -----

    [Fact]
    public async Task CreateEventAsync_CallsRepositoryAddExactlyOnce()
    {
        _mockEventRepository
            .Setup(r => r.AddAsync(It.IsAny<Event>(), It.IsAny<IEnumerable<Guid>>(), It.IsAny<IEnumerable<Guid>>()))
            .Returns(Task.CompletedTask);

        EventViewModel vmEvent = new EventViewModel
        {
            Name = "New Event",
            Date = DateTime.Today.AddDays(14),
            StartTime = new TimeSpan(10, 0, 0),
            EndTime = new TimeSpan(12, 0, 0),
            Description = "Test",
            MaxCapacity = 50,
            SelectedVenueIds = new List<Guid>(),
            SelectedActivityIds = new List<Guid>()
        };

        await _eventService.CreateEventAsync(vmEvent);

        _mockEventRepository.Verify(
            r => r.AddAsync(It.IsAny<Event>(), It.IsAny<IEnumerable<Guid>>(), It.IsAny<IEnumerable<Guid>>()),
            Times.Once);
    }

    // ----- UpdateEventAsync edge cases -----

    [Fact]
    public async Task UpdateEventAsync_WhenViewModelHasNoId_ThrowsEventManagementException()
    {
        // Guard: the form must have an Id to know which event to update.
        EventViewModel vmNoId = new EventViewModel { Id = null, Name = "X", MaxCapacity = 1 };

        await Assert.ThrowsAsync<EventManagementException>(async () =>
            await _eventService.UpdateEventAsync(vmNoId));
    }

    [Fact]
    public async Task UpdateEventAsync_WhenNewCapacityIsBelowActiveRegistrations_ThrowsEventManagementException()
    {
        // Edge case: admin tries to reduce capacity below the current confirmed count.
        Event existingEvent = new Event("Full", DateTime.Today.AddDays(7),
            new TimeSpan(9, 0, 0), new TimeSpan(10, 0, 0), "d", 5);
        existingEvent.AddRegistration(new Participant("A", "B", "a@b.com", "0700"), "Confirmed");
        existingEvent.AddRegistration(new Participant("C", "D", "c@d.com", "0701"), "Confirmed");

        _mockEventRepository.Setup(r => r.GetByIdAsync(existingEvent.Id)).ReturnsAsync(existingEvent);

        EventViewModel vmNewCapacity = new EventViewModel
        {
            Id = existingEvent.Id,
            Name = "Full",
            Date = existingEvent.Date,
            StartTime = existingEvent.StartTime,
            EndTime = existingEvent.EndTime,
            Description = "d",
            MaxCapacity = 1,   // 2 active registrations exist; 1 < 2 must be rejected
            SelectedVenueIds = new List<Guid>(),
            SelectedActivityIds = new List<Guid>()
        };

        await Assert.ThrowsAsync<EventManagementException>(async () =>
            await _eventService.UpdateEventAsync(vmNewCapacity));
    }

    [Fact]
    public async Task UpdateEventAsync_WhenCapacityMatchesExactActiveCount_IsAllowed()
    {
        // Boundary: reducing to exactly the confirmed count (not below) must succeed.
        Event existingEvent = new Event("Exact", DateTime.Today.AddDays(7),
            new TimeSpan(9, 0, 0), new TimeSpan(10, 0, 0), "d", 10);
        existingEvent.AddRegistration(new Participant("A", "B", "a@b.com", "0700"), "Confirmed");

        _mockEventRepository.Setup(r => r.GetByIdAsync(existingEvent.Id)).ReturnsAsync(existingEvent);
        _mockEventRepository
            .Setup(r => r.UpdateAsync(It.IsAny<Event>(), It.IsAny<IEnumerable<Guid>>(), It.IsAny<IEnumerable<Guid>>()))
            .Returns(Task.CompletedTask);

        EventViewModel vm = new EventViewModel
        {
            Id = existingEvent.Id,
            Name = "Exact",
            Date = existingEvent.Date,
            StartTime = existingEvent.StartTime,
            EndTime = existingEvent.EndTime,
            Description = "d",
            MaxCapacity = 1,   // exactly equals the one active registration — should be fine
            SelectedVenueIds = new List<Guid>(),
            SelectedActivityIds = new List<Guid>()
        };

        // Must not throw.
        await _eventService.UpdateEventAsync(vm);

        _mockEventRepository.Verify(
            r => r.UpdateAsync(It.IsAny<Event>(), It.IsAny<IEnumerable<Guid>>(), It.IsAny<IEnumerable<Guid>>()),
            Times.Once);
    }

    // ----- DeleteEventAsync -----

    [Fact]
    public async Task DeleteEventAsync_CallsRepositoryDeleteExactlyOnce()
    {
        Guid guidId = Guid.NewGuid();
        _mockEventRepository.Setup(r => r.DeleteAsync(guidId)).Returns(Task.CompletedTask);

        await _eventService.DeleteEventAsync(guidId);

        _mockEventRepository.Verify(r => r.DeleteAsync(guidId), Times.Once);
    }

    // ----- CancelEventAsync edge case -----

    [Fact]
    public async Task CancelEventAsync_WhenEventDoesNotExist_ThrowsEventNotFoundException()
    {
        _mockEventRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Event?)null);

        await Assert.ThrowsAsync<EventNotFoundException>(async () =>
            await _eventService.CancelEventAsync(Guid.NewGuid(), "gone"));
    }
}
