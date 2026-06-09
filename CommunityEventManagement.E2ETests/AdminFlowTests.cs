using Microsoft.Playwright;

namespace CommunityEventManagement.E2ETests;

/// <summary>
/// End-to-end tests for the Administrator journey. These exercise the back-office flows the same way
/// a real admin would in a browser: signing in and creating an event.
/// The CSS/role selectors target the application's known UI (Login.razor, the admin dashboard and
/// the event form); if the markup is tweaked, only the selectors here need updating.
/// </summary>
public class AdminFlowTests : PlaywrightTestBase
{
    /// <summary>
    /// E2E-01: an administrator can log in and land on the dashboard.
    /// </summary>
    [E2EFact]
    public async Task Admin_Can_Log_In_And_See_Dashboard()
    {
        await LoginAsync(sAdminEmail, sAdminPassword);

        // The admin dashboard shows an "Upcoming events" panel that no other role sees.
        await Expect(Page.GetByText("Upcoming events")).ToBeVisibleAsync();
    }

    /// <summary>
    /// E2E-02: an administrator can create a new event and see it confirmed.
    /// </summary>
    [E2EFact]
    public async Task Admin_Can_Create_An_Event()
    {
        await LoginAsync(sAdminEmail, sAdminPassword);

        // Go straight to the create-event form.
        await Page.GotoAsync($"{sBaseUrl}/admin/events/create");

        // Fill the core fields. The labels map to the EventForm (Name, Description, Max capacity);
        // the date and time inputs are standard HTML inputs.
        string sEventName = "E2E Spring Fair";
        await Page.GetByLabel("Name").FillAsync(sEventName);
        await Page.GetByLabel("Description").FillAsync("Created by an automated end-to-end test.");
        await Page.Locator("input[type='date']").FillAsync("2030-06-01");
        await Page.Locator("input[type='time']").First.FillAsync("09:00");
        await Page.Locator("input[type='time']").Last.FillAsync("12:00");
        await Page.GetByLabel("Maximum capacity").FillAsync("50");

        // Save the event.
        await Page.GetByRole(AriaRole.Button, new() { Name = "Save" }).ClickAsync();

        // After saving I should be back on the events list with my new event shown.
        await Expect(Page.GetByText(sEventName)).ToBeVisibleAsync();
    }
}
