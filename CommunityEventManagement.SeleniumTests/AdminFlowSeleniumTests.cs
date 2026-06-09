using OpenQA.Selenium;

namespace CommunityEventManagement.SeleniumTests;

/// <summary>
/// Selenium end-to-end tests for the Administrator. They drive a real browser the same way an admin
/// would: signing in, and proving the event form's validation fires.
/// </summary>
public class AdminFlowSeleniumTests : SeleniumTestBase
{
    /// <summary>
    /// S-01: an administrator can log in and land on the dashboard.
    /// </summary>
    [SeleniumFact]
    public void Admin_Can_Log_In_And_See_Dashboard()
    {
        Login(sAdminEmail, sAdminPassword);

        // The admin dashboard shows a "Quick actions" panel and an "Upcoming events" list that no
        // other role sees.
        WaitForText("Quick actions");
        Assert.Contains("Upcoming events", _wdDriver.PageSource);
    }

    /// <summary>
    /// S-02: submitting the New Event form while it is empty shows validation errors and does NOT
    /// navigate away — proving FluentValidation is wired into the form.
    /// </summary>
    [SeleniumFact]
    public void Admin_Sees_Validation_Errors_On_Empty_Event_Form()
    {
        Login(sAdminEmail, sAdminPassword);
        _wdDriver.Navigate().GoToUrl($"{sBaseUrl}/admin/events/create");

        // Click Save without filling anything in.
        Click(By.CssSelector("button[type='submit']"));

        // A validation message (rendered with the "text-danger" class) should appear, and we should
        // still be on the create page because the invalid form was not submitted.
        IWebElement weError = WaitForElement(By.CssSelector(".text-danger"));
        Assert.True(weError.Displayed);
        Assert.Contains("/admin/events/create", _wdDriver.Url);

        // TO EXTEND THIS INTO A FULL "create event" test: fill the fields by label, e.g.
        //   WaitForElement(By.XPath("//label[normalize-space()='Name']/following-sibling::input")).SendKeys("My Event");
        // then set the Date / Start time / End time inputs and tick a venue + activity checkbox in
        // the ".picker-box" boxes before clicking Save. (Date/time inputs are the fiddly part — they
        // are browser-locale sensitive, which is a good next thing to learn.)
    }
}
