using CommunityEventManagement.Application.Services;
using CommunityEventManagement.Domain.Entities;
using CommunityEventManagement.Domain.Exceptions;
using CommunityEventManagement.Domain.Interfaces;
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
}
