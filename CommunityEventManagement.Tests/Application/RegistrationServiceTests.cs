using CommunityEventManagement.Application.Services;
using CommunityEventManagement.Domain.Entities;
using CommunityEventManagement.Domain.Exceptions;
using CommunityEventManagement.Domain.Interfaces;
using Moq;

namespace CommunityEventManagement.Tests.Application;

/// <summary>
/// These tests check the RegistrationService business logic. They use Moq to create fake
/// repositories, so the tests run completely in memory with no database at all. That lets me focus
/// purely on whether the service makes the right decisions and throws the right exceptions.
/// </summary>
public class RegistrationServiceTests
{
    private readonly Mock<IEventRepository> _mockEventRepository = new();
    private readonly Mock<IParticipantRepository> _mockParticipantRepository = new();
    private readonly Mock<IRegistrationRepository> _mockRegistrationRepository = new();
    private readonly RegistrationService _registrationService;

    public RegistrationServiceTests()
    {
        _registrationService = new RegistrationService(
            _mockEventRepository.Object,
            _mockParticipantRepository.Object,
            _mockRegistrationRepository.Object);
    }

    [Fact]
    public async Task RegisterAsync_WhenEverythingIsValid_SavesTheRegistration()
    {
        // Arrange — an event with space and a participant who has not registered yet.
        Event targetEvent = new Event("Gala", DateTime.Today.AddDays(3), new TimeSpan(18, 0, 0), new TimeSpan(21, 0, 0), "x", 10);
        Participant participant = new Participant("Jo", "King", "jo@example.com", "0700");

        _mockEventRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(targetEvent);
        _mockParticipantRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(participant);
        _mockRegistrationRepository.Setup(r => r.AddAsync(It.IsAny<Registration>())).Returns(Task.CompletedTask);

        // Act
        Registration result = await _registrationService.RegisterAsync(targetEvent.Id, participant.Id);

        // Assert — a registration was returned and the repository was asked to save exactly one.
        Assert.Equal(participant.Id, result.ParticipantId);
        _mockRegistrationRepository.Verify(r => r.AddAsync(It.IsAny<Registration>()), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_WhenParticipantAlreadyRegistered_ThrowsDuplicateRegistrationException()
    {
        // Arrange — an event that the participant is already registered for.
        Event targetEvent = new Event("Gala", DateTime.Today.AddDays(3), new TimeSpan(18, 0, 0), new TimeSpan(21, 0, 0), "x", 10);
        Participant participant = new Participant("Jo", "King", "jo@example.com", "0700");
        targetEvent.AddRegistration(participant, "Confirmed");

        _mockEventRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(targetEvent);
        _mockParticipantRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(participant);

        // Act & Assert
        await Assert.ThrowsAsync<DuplicateRegistrationException>(async () =>
            await _registrationService.RegisterAsync(targetEvent.Id, participant.Id));
    }

    [Fact]
    public async Task RegisterAsync_WhenEventIsFull_ThrowsVenueCapacityExceededException()
    {
        // Arrange — an event with capacity 1 that already has one registration.
        Event targetEvent = new Event("Small Room", DateTime.Today.AddDays(3), new TimeSpan(18, 0, 0), new TimeSpan(19, 0, 0), "x", 1);
        Participant firstPerson = new Participant("Al", "One", "al@example.com", "0701");
        targetEvent.AddRegistration(firstPerson, "Confirmed");

        Participant secondPerson = new Participant("Bea", "Two", "bea@example.com", "0702");
        _mockEventRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(targetEvent);
        _mockParticipantRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(secondPerson);

        // Act & Assert — the second person cannot fit, so the capacity exception is thrown.
        await Assert.ThrowsAsync<VenueCapacityExceededException>(async () =>
            await _registrationService.RegisterAsync(targetEvent.Id, secondPerson.Id));
    }
}
