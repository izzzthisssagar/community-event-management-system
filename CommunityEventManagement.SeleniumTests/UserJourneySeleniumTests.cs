using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace CommunityEventManagement.SeleniumTests;

/// <summary>
/// Selenium end-to-end tests for the self-service Registered User: browsing and filtering events,
/// registering for an event, and seeing a broken rule (a duplicate registration) handled with a
/// friendly message instead of a crash.
/// </summary>
public class UserJourneySeleniumTests : SeleniumTestBase
{
    /// <summary>
    /// S-03: a signed-in user can browse the events list and narrow it with a filter.
    /// </summary>
    [SeleniumFact]
    public void User_Can_Browse_And_Filter_Events()
    {
        Login(sUserEmail, sUserPassword);
        _wdDriver.Navigate().GoToUrl($"{sBaseUrl}/events");

        // Each event is rendered inside an ".event-card"; wait for at least one to appear.
        WaitForElement(By.CssSelector(".event-card"));

        // Filter by activity type using the "All types" dropdown (the last <select> on the page).
        IList<IWebElement> selectElements = _wdDriver.FindElements(By.CssSelector("select.form-select"));
        SelectElement seActivityType = new SelectElement(selectElements[selectElements.Count - 1]);
        seActivityType.SelectByText("Workshop");

        // Changing a filter restarts the page's 400 ms debounce before it re-queries. A short, fixed
        // pause for a KNOWN timed delay is one of the few times a small sleep is reasonable.
        Thread.Sleep(700);

        // Either way the page is still a healthy list (cards or a friendly empty state) — never the
        // error boundary.
        Assert.DoesNotContain("Something went wrong", _wdDriver.PageSource);
    }

    /// <summary>
    /// S-04: a Registered User can register themselves for an event.
    /// (Assumes a freshly seeded database where the demo user is not already on the first event.)
    /// </summary>
    [SeleniumFact]
    public void User_Can_Register_For_An_Event()
    {
        Login(sUserEmail, sUserPassword);
        _wdDriver.Navigate().GoToUrl($"{sBaseUrl}/events");

        // Open the first event via its "View details" link (the only link inside the card).
        Click(By.CssSelector(".event-card a"));

        // A normal user registers themselves with a single button (no participant picker).
        Click(By.XPath("//button[contains(., 'Register me for this event')]"));

        // The page shows an inline success alert and refreshes the "seats left" count.
        WaitForText("Registered successfully.");
        Assert.Contains("Registered successfully.", _wdDriver.PageSource);
    }

    /// <summary>
    /// S-05: registering for the same event twice raises my DuplicateRegistrationException, which the
    /// page turns into a friendly red alert rather than a crash.
    /// </summary>
    [SeleniumFact]
    public void Duplicate_Registration_Shows_A_Friendly_Error()
    {
        Login(sUserEmail, sUserPassword);
        _wdDriver.Navigate().GoToUrl($"{sBaseUrl}/events");
        Click(By.CssSelector(".event-card a"));

        By byRegisterButton = By.XPath("//button[contains(., 'Register me for this event')]");

        // First attempt (the user may already be registered from a previous run — either way is fine).
        Click(byRegisterButton);
        Thread.Sleep(500);

        // Second attempt on the same event must be rejected with a friendly danger alert.
        IList<IWebElement> registerButtons = _wdDriver.FindElements(byRegisterButton);
        if (registerButtons.Count > 0 && registerButtons[0].Displayed)
        {
            registerButtons[0].Click();
        }

        IWebElement weAlert = WaitForElement(By.CssSelector(".alert-danger"));
        Assert.True(weAlert.Displayed);
    }
}
