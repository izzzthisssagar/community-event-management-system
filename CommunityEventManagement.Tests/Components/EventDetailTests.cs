using Bunit;
using Bunit.TestDoubles;
using CommunityEventManagement.Application.Services;
using CommunityEventManagement.Components.Pages.User;
using CommunityEventManagement.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace CommunityEventManagement.Tests.Components;

/// <summary>
/// bUnit tests for the EventDetail page, including its participant registration form.
/// </summary>
public class EventDetailTests : TestContext
{
    private Mock<IEventService> _mockEventService = new();
    private Mock<IParticipantService> _mockParticipantService = new();
    private Mock<IRegistrationService> _mockRegistrationService = new();

    private Event ArrangeServices(int iMaxCapacity = 100)
    {
        Event testEvent = new Event("Charity Concert", DateTime.Today.AddDays(5),
            new TimeSpan(19, 0, 0), new TimeSpan(22, 0, 0), "An evening of music", iMaxCapacity);

        _mockEventService.Setup(s => s.GetEventByIdAsync(It.IsAny<Guid>())).ReturnsAsync(testEvent);

        // Provide one participant so the registration form renders.
        _mockParticipantService.Setup(s => s.GetAllAsync())
            .ReturnsAsync(new List<Participant> { new Participant("Mia", "Stone", "mia@example.com", "0700") });

        Services.AddSingleton(_mockEventService.Object);
        Services.AddSingleton(_mockParticipantService.Object);
        Services.AddSingleton(_mockRegistrationService.Object);

        // Render as an admin so the participant picker (and its "choose a participant" validation)
        // is shown. bUnit supplies the cascading AuthenticationState that the page reads.
        TestAuthorizationContext authContext = this.AddTestAuthorization();
        authContext.SetAuthorized("System Administrator");
        authContext.SetRoles("Admin");

        return testEvent;
    }

    [Fact]
    public void EventDetail_ShowsTheEventName()
    {
        // Arrange
        Event testEvent = ArrangeServices();

        // Act
        IRenderedComponent<EventDetail> cut = RenderComponent<EventDetail>(
            parameters => parameters.Add(p => p.EventId, testEvent.Id));

        // Assert — the event name appears somewhere on the page.
        Assert.Contains("Charity Concert", cut.Markup);
    }

    [Fact]
    public void EventDetail_ClickingRegisterWithoutChoosingParticipant_ShowsAnError()
    {
        // Arrange
        Event testEvent = ArrangeServices();
        IRenderedComponent<EventDetail> cut = RenderComponent<EventDetail>(
            parameters => parameters.Add(p => p.EventId, testEvent.Id));

        // Act — click Register while the participant dropdown is still on "-- select --".
        cut.Find("button").Click();

        // Assert — a friendly error message is shown and no registration was attempted.
        Assert.Contains("Please choose a participant first", cut.Markup);
        _mockRegistrationService.Verify(s => s.RegisterAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
    }
}
