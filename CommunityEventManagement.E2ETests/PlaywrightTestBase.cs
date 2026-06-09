using Microsoft.Playwright;
using Microsoft.Playwright.Xunit;

namespace CommunityEventManagement.E2ETests;

/// <summary>
/// Base class for every end-to-end (E2E) test. It inherits Microsoft.Playwright.Xunit's
/// <see cref="PageTest"/>, which automatically gives each test a fresh browser, a fresh browser
/// context and a fresh <c>Page</c>, plus the <c>Expect(...)</c> assertion helper (which auto-retries
/// until the UI catches up). I keep the shared constants — the address the app runs on and the two
/// demo accounts that the seeder creates — together with a small login helper here, so the actual
/// test classes stay short and easy to read.
///
/// NOTE: These tests drive the REAL running application through a REAL browser. Before running them
/// you must (1) start the app (<c>dotnet run --project CommunityEventManagement</c>) and (2) install
/// the Playwright browser binaries once (<c>playwright.ps1 install</c>). See CEMS_Test_Plan_v1.0.md.
/// </summary>
public abstract class PlaywrightTestBase : PageTest
{
    // The address the application is served from when it is started with the "http" launch profile.
    // If you run the app on a different port, change this one constant.
    protected const string sBaseUrl = "http://localhost:5131";

    // The two demo accounts that DbSeeder creates on first run.
    protected const string sAdminEmail = "admin@events.com";
    protected const string sAdminPassword = "Admin123!";
    protected const string sUserEmail = "user@events.com";
    protected const string sUserPassword = "User123!";

    /// <summary>
    /// Signs a user in through the real login form. The login page is rendered statically and the
    /// form does a normal HTTP POST to my AuthController at /auth/login, so I fill the inputs and
    /// click the submit button, then wait for the resulting navigation to settle.
    /// </summary>
    protected async Task LoginAsync(string sEmail, string sPassword)
    {
        await Page.GotoAsync($"{sBaseUrl}/login");

        // The login inputs have id="email" and id="password" (see Login.razor).
        await Page.Locator("#email").FillAsync(sEmail);
        await Page.Locator("#password").FillAsync(sPassword);

        // The submit button is labelled "Sign in".
        await Page.GetByRole(AriaRole.Button, new() { Name = "Sign in" }).ClickAsync();

        // The POST redirects back into the app; wait until the network is quiet before asserting.
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }
}
