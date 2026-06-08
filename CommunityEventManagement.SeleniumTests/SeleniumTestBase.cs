using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace CommunityEventManagement.SeleniumTests;

/// <summary>
/// Base class for every Selenium end-to-end test. xUnit creates a NEW instance of the test class for
/// each test method, so each test gets its own fresh Chrome browser here in the constructor and
/// closes it again in <see cref="Dispose"/>. The shared constants (the app URL, the demo accounts)
/// and the small reusable helpers (waiting for elements, logging in) live here so the individual
/// test classes stay short and readable.
///
/// LEARNING NOTES (this is the part worth studying):
///   • Selenium controls a real browser through the <c>IWebDriver</c> interface (here, ChromeDriver).
///   • Selenium 4.6+ ships "Selenium Manager", which downloads the matching ChromeDriver for you the
///     first time — so you only need Google Chrome installed, no manual driver download.
///   • The golden rule is to use an EXPLICIT WAIT (<c>WebDriverWait</c>) instead of <c>Thread.Sleep</c>.
///     An explicit wait keeps re-checking a condition until it is true (or it times out), which makes
///     tests reliable on a slow or fast machine alike.
///
/// PRE-REQUISITES to actually run these (they are skipped by default — see SeleniumFactAttribute):
///   1. Google Chrome installed.
///   2. The app running:  dotnet run --project CommunityEventManagement  (http://localhost:5131)
///   3. RUN_SELENIUM=1 set in the environment.
/// </summary>
public abstract class SeleniumTestBase : IDisposable
{
    // The address the application is served from with the "http" launch profile.
    protected const string sBaseUrl = "http://localhost:5131";

    // The two demo accounts the seeder creates on first run.
    protected const string sAdminEmail = "admin@events.com";
    protected const string sAdminPassword = "Admin123!";
    protected const string sUserEmail = "user@events.com";
    protected const string sUserPassword = "User123!";

    // _wdDriver is the browser Selenium is driving; _wdWait is my reusable explicit wait.
    protected readonly IWebDriver _wdDriver;
    protected readonly WebDriverWait _wdWait;

    protected SeleniumTestBase()
    {
        // Configure Chrome. Selenium Manager resolves the driver automatically.
        ChromeOptions coOptions = new ChromeOptions();

        // "--headless=new" runs Chrome WITHOUT a visible window (faster, and works on a build server).
        // TIP while learning: comment out this next line to actually WATCH the browser drive itself.
        coOptions.AddArgument("--headless=new");
        coOptions.AddArgument("--disable-gpu");
        coOptions.AddArgument("--window-size=1440,900");

        _wdDriver = new ChromeDriver(coOptions);

        // The explicit wait: keep re-checking a condition for up to 10 seconds before giving up.
        _wdWait = new WebDriverWait(_wdDriver, TimeSpan.FromSeconds(10));

        // While waiting, treat "element not found yet" and "element went stale" as "not ready yet"
        // (keep retrying) rather than as a hard failure.
        _wdWait.IgnoreExceptionTypes(typeof(NoSuchElementException), typeof(StaleElementReferenceException));
    }

    /// <summary>
    /// Waits until the element located by <paramref name="by"/> exists and is visible, then returns
    /// it. This is the helper I reach for instead of <c>FindElement</c> + <c>Thread.Sleep</c>.
    /// </summary>
    protected IWebElement WaitForElement(By by)
    {
        return _wdWait.Until(driver =>
        {
            IWebElement weElement = driver.FindElement(by);
            // Returning null tells WebDriverWait "not ready yet, keep waiting".
            return weElement.Displayed ? weElement : null;
        })!;
    }

    /// <summary>Waits for an element, then clicks it.</summary>
    protected void Click(By by)
    {
        WaitForElement(by).Click();
    }

    /// <summary>Waits until the page's HTML contains <paramref name="sText"/>.</summary>
    protected void WaitForText(string sText)
    {
        _wdWait.Until(driver => driver.PageSource.Contains(sText));
    }

    /// <summary>
    /// Signs a user in through the real login form (a static HTML form that POSTs to /auth/login),
    /// then waits until the resulting redirect has taken us away from the /login page.
    /// </summary>
    protected void Login(string sEmail, string sPassword)
    {
        _wdDriver.Navigate().GoToUrl($"{sBaseUrl}/login");

        // The login inputs have id="email" and id="password" (see Login.razor).
        WaitForElement(By.Id("email")).SendKeys(sEmail);
        _wdDriver.FindElement(By.Id("password")).SendKeys(sPassword);

        // Click the "Sign in" submit button.
        _wdDriver.FindElement(By.CssSelector("button[type='submit']")).Click();

        // Wait until we have left the login page (the POST redirects us back into the app).
        _wdWait.Until(driver => !driver.Url.Contains("/login"));
    }

    /// <summary>Closes the browser after each test, even if the test failed.</summary>
    public void Dispose()
    {
        _wdDriver.Quit();
        _wdDriver.Dispose();
    }
}
