namespace CommunityEventManagement.SeleniumTests;

/// <summary>
/// A drop-in replacement for <c>[Fact]</c> that SKIPS the test unless the <c>RUN_SELENIUM</c>
/// environment variable is set to "1". The Selenium tests open a real Chrome browser and drive the
/// REAL running application, which is not available during a normal <c>dotnet test</c> / CI run. By
/// marking each test with <c>[SeleniumFact]</c> they are reported as <b>skipped</b> (never failed) by
/// default, so the main 43-test unit suite and the whole solution build stay completely green while
/// these learning/automation scripts still live in the repository.
///
/// To actually run them: start the app, set <c>RUN_SELENIUM=1</c>, then run
/// <c>dotnet test CommunityEventManagement.SeleniumTests</c>. See CEMS_Getting_Started.md.
/// </summary>
public sealed class SeleniumFactAttribute : FactAttribute
{
    public SeleniumFactAttribute()
    {
        // Read the opt-in flag at test-discovery time; when it is missing, set Skip so xUnit marks
        // the test as skipped instead of trying to open Chrome and reach a server that is not up.
        if (Environment.GetEnvironmentVariable("RUN_SELENIUM") != "1")
        {
            Skip = "Selenium test skipped by default. Start the app, then set RUN_SELENIUM=1 to enable.";
        }
    }
}
