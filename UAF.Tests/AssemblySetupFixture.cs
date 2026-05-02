using UAF.Core.Driver;

namespace UAF.Tests;

/// <summary>
/// NUnit assembly-level setup and teardown. Runs once before any test
/// in the assembly executes and once after all tests complete.
/// Responsible for shared browser lifecycle only — per-test setup
/// lives in <c>BaseTest</c>.
/// </summary>
[SetUpFixture]
public class AssemblySetupFixture
{
    /// <summary>
    /// Initializes the shared Playwright browser before any test runs.
    /// </summary>
    [OneTimeSetUp]
    public async Task RunBeforeAnyTests()
    {
        await DriverManager.InitializeBrowserAsync();
    }

    /// <summary>
    /// Disposes the shared Playwright browser after all tests complete.
    /// </summary>
    [OneTimeTearDown]
    public async Task RunAfterAllTests()
    {
        await DriverManager.DisposeBrowserAsync();
    }
}
