using Serilog;
using UAF.Core.Config;
using UAF.Core.Driver;
using UAF.Core.Logging;

namespace UAF.Tests;

/// <summary>
/// NUnit assembly-level setup and teardown. Runs once before the first test
/// and once after the last test in the assembly.
/// Owns two cross-cutting concerns that must be initialized once per run:
/// Serilog logging and the shared Playwright browser.
/// </summary>
[SetUpFixture]
public class AssemblySetupFixture
{
    /// <summary>
    /// Bootstraps Serilog logging before any test runs.
    /// Log level and output directory are read from <see cref="ConfigManager"/>
    /// so that consumers control the verbosity via their own
    /// <c>appsettings.json</c>.
    /// </summary>
    [OneTimeSetUp]
    public void RunBeforeAllTests()
    {
        var settings     = ConfigManager.Instance.Settings;
        var logDirectory = Path.Combine(AppContext.BaseDirectory, "logs");

        LoggerBootstrapper.Initialize(settings.Logging.LogLevel, logDirectory);
    }

    /// <summary>
    /// Disposes the shared Playwright browser and flushes all pending log
    /// writes after all tests complete.
    /// Browser disposal runs first so that any teardown log entries from
    /// <c>DriverManager</c> are captured before the logger is flushed.
    /// Safe to call when no browser was initialized — unit-only runs are a
    /// no-op for browser disposal.
    /// </summary>
    [OneTimeTearDown]
    public async Task RunAfterAllTests()
    {
        await DriverManager.DisposeBrowserAsync();
        Log.CloseAndFlush();
    }
}