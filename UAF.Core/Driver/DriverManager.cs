using Microsoft.Playwright;
using Serilog;
using UAF.Core.Config;
using BrowserType = UAF.Core.Driver.BrowserType;

namespace UAF.Core.Driver;

/// <summary>
/// Static factory responsible for Playwright browser lifecycle.
/// Holds the shared <see cref="IBrowser"/> instance which is initialized
/// once at assembly level via <c>AssemblySetupFixture</c>.
/// Per-test page and context lifecycle is the responsibility of
/// <c>BaseTest</c> — DriverManager only creates and disposes them on request.
/// </summary>
public static class DriverManager
{
    private static IBrowser? _browser;

    /// <summary>
    /// Initializes the shared browser before any test runs.
    /// Reads browser type and headless mode from <see cref="ConfigManager"/>.
    /// Safe to call multiple times — subsequent calls are no-ops if the
    /// browser is already running.
    /// </summary>
    public static async Task InitializeBrowserAsync()
    {
        if (_browser is not null)
            return;

        var settings = ConfigManager.Instance.Settings;

        if (!Enum.TryParse<BrowserType>(settings.Browser.Browser, ignoreCase: true, out var browserType))
            throw new InvalidOperationException(
                $"'{settings.Browser.Browser}' is not a supported browser type. " +
                $"Valid values: {string.Join(", ", Enum.GetNames<BrowserType>())}");

        _browser = await DriverFactory.CreateBrowserAsync(browserType, settings.Browser.Headless);
    }

    /// <summary>
    /// Creates a new isolated browser context and page for a single test.
    /// Call from <c>BaseTest</c> <c>[SetUp]</c>. Each call returns a fresh
    /// <see cref="IPage"/> with no state shared with other tests.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when called before <see cref="InitializeBrowserAsync"/>.
    /// </exception>
    public static async Task<IPage> CreatePageAsync()
    {
        if (_browser is null)
            throw new InvalidOperationException(
                "Browser has not been initialized. " +
                "Ensure AssemblySetupFixture runs before any test.");

        var context = await _browser.NewContextAsync();
        return await context.NewPageAsync();
    }

    /// <summary>
    /// Disposes the page and its parent browser context.
    /// Call from <c>BaseTest</c> <c>[TearDown]</c> after each test completes.
    /// Null-safe — passing a null page is silently ignored.
    /// </summary>
    public static async Task ClosePageAsync(IPage? page)
    {
        if (page is null)
            return;

        // Disposing the context also closes the page cleanly.
        // Wrapped so that browser cleanup still runs if this fails.
        try
        {
            await page.Context.DisposeAsync();
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "[WARN] Browser context disposal failed — continuing teardown");
        }
    }

    /// <summary>
    /// Disposes the shared browser. Call once per test run from
    /// <c>AssemblySetupFixture</c> teardown — not per test.
    /// Safe to call when the browser is already null or disposed.
    /// </summary>
    public static async Task DisposeBrowserAsync()
    {
        try
        {
            if (_browser is not null)
                await _browser.DisposeAsync();
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "[WARN] Browser disposal failed — continuing cleanup");
        }
        finally
        {
            _browser = null;
        }
    }
}
