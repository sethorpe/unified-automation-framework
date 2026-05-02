using UAF.Core.Driver;

namespace UAF.Tests;

/// <summary>
/// NUnit assembly-level teardown. Runs once after all tests in the assembly
/// complete. Disposes the shared browser if any test initialized it.
/// Browser initialization is intentionally lazy — <c>BaseTest.[SetUp]</c>
/// calls <c>DriverManager.InitializeBrowserAsync()</c> so that CI pipelines
/// running only unit tests never launch a browser process.
/// </summary>
[SetUpFixture]
public class AssemblySetupFixture
{
    /// <summary>
    /// Disposes the shared Playwright browser after all tests complete.
    /// Safe to call when no browser was initialized — unit-only runs are a no-op.
    /// </summary>
    [OneTimeTearDown]
    public async Task RunAfterAllTests()
    {
        await DriverManager.DisposeBrowserAsync();
    }
}
