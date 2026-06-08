using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace CommunityEventManagement.E2ETests;

/// <summary>
/// End-to-end test for the public self-service sign-up form. This focuses on the validation side of
/// the journey — proving that the SignUpViewModelValidator stops a bad submission (mismatched
/// passwords) before any account is created.
/// </summary>
public class SignUpTests : PlaywrightTestBase
{
    /// <summary>
    /// E2E-06: the sign-up form blocks submission when the password and the confirmation do not
    /// match, and the user stays on the sign-up page.
    /// </summary>
    [E2EFact]
    public async Task SignUp_With_Mismatched_Passwords_Is_Blocked()
    {
        await Page.GotoAsync($"{sBaseUrl}/signup");

        // Fill the form but make the confirmation password deliberately different.
        await Page.GetByLabel(new Regex("full name", RegexOptions.IgnoreCase)).FillAsync("Test Person");
        await Page.GetByLabel("Email").FillAsync("brand-new-user@example.com");
        await Page.GetByLabel(new Regex("^password$", RegexOptions.IgnoreCase)).First.FillAsync("Password123!");
        await Page.GetByLabel(new Regex("confirm", RegexOptions.IgnoreCase)).FillAsync("DifferentPassword!");

        // Try to submit (the form's submit button).
        await Page.Locator("button[type='submit']").ClickAsync();

        // The validator should keep us on /signup and show a validation message; no account is made.
        await Expect(Page).ToHaveURLAsync(new Regex("/signup"));
        await Expect(Page.Locator(".validation-message, .text-danger, .invalid-feedback").First).ToBeVisibleAsync();
    }
}
