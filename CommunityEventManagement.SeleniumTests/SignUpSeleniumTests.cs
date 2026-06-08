using OpenQA.Selenium;

namespace CommunityEventManagement.SeleniumTests;

/// <summary>
/// Selenium end-to-end test for the public sign-up form, focused on validation: a mismatched
/// confirm-password must be rejected before any account is created.
/// </summary>
public class SignUpSeleniumTests : SeleniumTestBase
{
    /// <summary>
    /// S-06: the sign-up form blocks submission when the password and the confirmation differ, shows
    /// a validation message, and keeps the user on /signup.
    /// </summary>
    [SeleniumFact]
    public void SignUp_With_Mismatched_Passwords_Shows_Validation()
    {
        _wdDriver.Navigate().GoToUrl($"{sBaseUrl}/signup");

        // Fill the named text fields. These inputs have no id, so I locate each one by the text of
        // the <label> immediately before it — a handy XPath pattern when there is no id to target.
        FillByLabel("First name", "Test");
        FillByLabel("Last name", "Person");
        FillByLabel("Email", "brand-new-user@example.com");
        FillByLabel("Phone number", "0123456789");

        // The two password boxes are the only inputs of type="password" on the page: [0] = Password,
        // [1] = Confirm password. I deliberately make them different.
        IList<IWebElement> passwordInputs = _wdDriver.FindElements(By.CssSelector("input[type='password']"));
        passwordInputs[0].SendKeys("Password123!");
        passwordInputs[1].SendKeys("DoesNotMatch!");

        // Submit the form.
        Click(By.CssSelector("button[type='submit']"));

        // The confirm-password rule should fail: a validation message appears and we stay on /signup
        // (no account is created).
        IWebElement weValidation = WaitForElement(By.CssSelector(".text-danger"));
        Assert.True(weValidation.Displayed);
        Assert.Contains("/signup", _wdDriver.Url);
    }

    /// <summary>Locates an input by the text of the &lt;label&gt; directly before it, then types into it.</summary>
    private void FillByLabel(string sLabel, string sValue)
    {
        IWebElement weInput = WaitForElement(By.XPath($"//label[normalize-space()='{sLabel}']/following-sibling::input"));
        weInput.SendKeys(sValue);
    }
}
