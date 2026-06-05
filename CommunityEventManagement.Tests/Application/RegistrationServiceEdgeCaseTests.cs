using CommunityEventManagement.Application.Services;
using CommunityEventManagement.Domain.Entities;
using CommunityEventManagement.Domain.Exceptions;
using CommunityEventManagement.Domain.Interfaces;
using Moq;

namespace CommunityEventManagement.Tests.Application;

/// <summary>
/// These tests cover the less obvious error paths of the RegistrationService, so that every
/// failure scenario is checked, not just the happy path.
/// </summary>
public class RegistrationServiceEdgeCaseTests
{
    private readonly Mock<IEventRepository> _mockEventRepository = new();
    private readonly Mock<IParticipantRepository> _mockParticipantRepository = new();
    private readonly Mock<IRegistrationRepository> _mockRegistrationRepository = new();
    private readonly RegistrationService _service;

    public RegistrationServiceEdgeCaseTests()
    {
        _service = new RegistrationService(
            _mockEventRepository.Object, _mockParticipantRepository.Object, _mockRegistrationRepository.Object);
    }

    [Fact]
    public async Task RegisterAsync_WhenEventDoesNotExist_ThrowsEventNotFoundException()
    {
        _mockEventRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Event?)null);

        await Assert.ThrowsAsync<EventNotFoundException>(async () =>
            await _service.RegisterAsync(Guid.NewGuid(), Guid.NewGuid()));
    }

    [Fact]
    public async Task RegisterAsync_WhenParticipantDoesNotExist_ThrowsEntityNotFoundException()
    {
        Event anEvent = new Event("E", DateTime.Today.AddDays(1), new TimeSpan(9, 0, 0), new TimeSpan(10, 0, 0), "d", 10);
        _mockEventRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(anEvent);
        _mockParticipantRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Participant?)null);

        await Assert.ThrowsAsync<EntityNotFoundException>(async () =>
            await _service.RegisterAsync(anEvent.Id, Guid.NewGuid()));
    }

    [Fact]
    public async Task RegisterAsync_WhenEventIsCancelled_ThrowsEventManagementException()
    {
        Event cancelledEvent = new Event("E", DateTime.Today.AddDays(1), new TimeSpan(9, 0, 0), new TimeSpan(10, 0, 0), "d", 10);
        cancelledEvent.Cancel("Called off");
        Participant participant = new Participant("A", "B", "a@b.com", "0700");

        _mockEventRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(cancelledEvent);
        _mockParticipantRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(participant);

        EventManagementException ex = await Assert.ThrowsAsync<EventManagementException>(async () =>
            await _service.RegisterAsync(cancelledEvent.Id, participant.Id));
        Assert.Contains("cancelled", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CancelRegistrationAsync_CancelsTheRegistrationAndSavesIt()
    {
        Registration registration = new Registration(Guid.NewGuid(), Guid.NewGuid(), "Confirmed");
        _mockRegistrationRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(registration);
        _mockRegistrationRepository.Setup(r => r.UpdateAsync(It.IsAny<Registration>())).Returns(Task.CompletedTask);

        await _service.CancelRegistrationAsync(registration.Id, "Cannot attend");

        Assert.True(registration.IsCancelled);
        Assert.Equal("Cancelled", registration.Status);
        _mockRegistrationRepository.Verify(r => r.UpdateAsync(registration), Times.Once);
    }

    [Fact]
    public async Task CancelRegistrationAsync_WhenRegistrationNotFound_ThrowsEntityNotFoundException()
    {
        _mockRegistrationRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Registration?)null);

        await Assert.ThrowsAsync<EntityNotFoundException>(async () =>
            await _service.CancelRegistrationAsync(Guid.NewGuid(), "x"));
    }
}
