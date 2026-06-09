namespace CommunityEventManagement.E2ETests;

/// <summary>
/// A drop-in replacement for <c>[Fact]</c> that SKIPS the test unless the <c>RUN_E2E</c> environment
/// variable is set to "1". The end-to-end tests need the application running AND the Playwright
/// browser binaries installed, neither of which is true during a normal <c>dotnet test</c> or CI
/// run. By marking them with <c>[E2EFact]</c> they are reported as <b>skipped</b> (never failed) by
/// default, so the main 43-test unit suite and the whole solution build stay completely green while
/// the automation scripts are still shipped in the repository.
///
/// To actually run them: start the app, run <c>playwright install</c> once, set <c>RUN_E2E=1</c>,
/// then run <c>dotnet test CommunityEventManagement.E2ETests</c>. See CEMS_Test_Plan_v1.0.md §5.
/// </summary>
public sealed class E2EFactAttribute : FactAttribute
{
    public E2EFactAttribute()
    {
        // Read the opt-in flag at test-discovery time; when it is missing, set Skip so xUnit marks
        // the test as skipped instead of trying to run it (which would need a browser and a server).
        if (Environment.GetEnvironmentVariable("RUN_E2E") != "1")
        {
            Skip = "E2E test skipped by default. Start the app, run 'playwright install', then set RUN_E2E=1 to enable.";
        }
    }
}
