using Bunit;
using CommunityEventManagement.Application.Services;
using CommunityEventManagement.Components.Pages.User;
using CommunityEventManagement.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace CommunityEventManagement.Tests.Components;

/// <summary>
/// These tests use bUnit to render my Blazor components in memory and check the HTML they produce.
/// This proves the user interface actually shows the data it is given. I use Moq to provide fake
/// services so the components do not need a real database.
/// </summary>
public class EventListTests : TestContext
{
    [Fact]
    public void EventList_WhenGivenTwoEvents_RendersTwoEventCards()
    {
        // Arrange — fake services that return two upcoming events and no venues.
        List<Event> twoEvents = new()
        {
            new Event("Fair", DateTime.Today.AddDays(2), new TimeSpan(10, 0, 0), new TimeSpan(12, 0, 0), "Fun", 100),
            new Event("Talk", DateTime.Today.AddDays(4), new TimeSpan(13, 0, 0), new TimeSpan(14, 0, 0), "Learn", 80)
        };

        Mock<IEventService> mockEventService = new();
        mockEventService.Setup(s => s.GetUpcomingEventsAsync()).ReturnsAsync(twoEvents);

        Mock<IVenueService> mockVenueService = new();
        mockVenueService.Setup(s => s.GetAllAsync()).ReturnsAsync(new List<Venue>());

        Services.AddSingleton(mockEventService.Object);
        Services.AddSingleton(mockVenueService.Object);

        // Act — render the browse page.
        IRenderedComponent<EventList> cut = RenderComponent<EventList>();

        // Assert — there should be exactly two event cards on the page.
        Assert.Equal(2, cut.FindAll(".event-card").Count);
    }
}
