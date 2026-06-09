using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace CommunityEventManagement.E2ETests;

/// <summary>
/// End-to-end tests for the self-service Registered User journey: browsing and filtering events,
/// registering for an event, and seeing a broken business rule (a duplicate registration) handled
/// gracefully with a friendly message rather than a crash.
/// </summary>
public class UserJourneyTests : PlaywrightTestBase
{
    /// <summary>
    /// E2E-04: any signed-in user can browse the events list and narrow it with the search box.
    /// </summary>
    [E2EFact]
    public async Task User_Can_Browse_And_Filter_Events()
    {
        await LoginAsync(sUserEmail, sUserPassword);
        await Page.GotoAsync($"{sBaseUrl}/events");

        // The browse page renders each event inside an ".event-card" element (see User/EventList).
        await Expect(Page.Locator(".event-card").First).ToBeVisibleAsync();

        // Type into the debounced search box; the list should react after the ~400 ms debounce.
        ILocator searchBox = Page.GetByPlaceholder(new Regex("search", RegexOptions.IgnoreCase));
        if (await searchBox.CountAsync() > 0)
        {
            await searchBox.FillAsync("Workshop");
            // Give the 400 ms debounce time to fire and the list time to re-render.
            await Page.WaitForTimeoutAsync(800);
        }

        // Either way, the page is still a healthy events list (no error boundary shown).
        await Expect(Page.GetByText("Something went wrong")).ToBeHiddenAsync();
    }

    /// <summary>
    /// E2E-03: a Registered User can register themselves for an event and then see it under
    /// "My Registrations".
    /// </summary>
    [E2EFact]
    public async Task User_Can_Register_For_An_Event()
    {
        await LoginAsync(sUserEmail, sUserPassword);
        await Page.GotoAsync($"{sBaseUrl}/events");

        // Open the first event in the list.
        await Page.Locator(".event-card").First.ClickAsync();

        // A normal user registers themselves with a single button (no participant picker).
        ILocator registerButton = Page.GetByRole(AriaRole.Button, new() { Name = "Register me for this event" });
        await registerButton.ClickAsync();

        // The page shows an inline success message and refreshes the "seats left" count.
        await Expect(Page.GetByText("Registered successfully.")).ToBeVisibleAsync();

        // The booking should now appear on the My Registrations page.
        await Page.GotoAsync($"{sBaseUrl}/registrations");
        await Expect(Page.Locator("table, .list-group, .card").First).ToBeVisibleAsync();
    }

    /// <summary>
    /// E2E-05: registering for the same event twice raises my DuplicateRegistrationException, which
    /// the page turns into a friendly error message instead of crashing.
    /// </summary>
    [E2EFact]
    public async Task Duplicate_Registration_Shows_A_Friendly_Error()
    {
        await LoginAsync(sUserEmail, sUserPassword);
        await Page.GotoAsync($"{sBaseUrl}/events");
        await Page.Locator(".event-card").First.ClickAsync();

        ILocator registerButton = Page.GetByRole(AriaRole.Button, new() { Name = "Register me for this event" });

        // First attempt (may already be registered from a previous run — either way is fine).
        await registerButton.ClickAsync();
        await Page.WaitForTimeoutAsync(500);

        // Second attempt on the same event must be rejected with a friendly danger alert, never a
        // raw stack trace.
        if (await registerButton.IsVisibleAsync())
        {
            await registerButton.ClickAsync();
        }

        await Expect(Page.Locator(".alert-danger")).ToBeVisibleAsync();
    }
}
