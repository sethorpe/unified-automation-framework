using Microsoft.Playwright;
using Serilog;
using UAF.Core.Config;
using BrowserType = UAF.Core.Driver.BrowserType;

namespace UAF.Core.Driver;

/// <summary>
/// Static lifecycle manager for the shared Playwright browser.
/// Owns both the <see cref="IPlaywright"/> runtime and the
/// <see cref="IBrowser"/> instance for the duration of a test run.
/// Per-test page and context lifecycle is delegated to <c>BaseTest</c>.
/// </summary>
/// <remarks>
/// Ownership model:
/// <list type="bullet">
///   <item><see cref="IPlaywright"/> is created here and disposed here.
///         <see cref="DriverFactory"/> never touches its lifetime.</item>
///   <item><see cref="IBrowser"/> is created via <see cref="DriverFactory"/>
///         and disposed before <see cref="IPlaywright"/> — reverse creation
///         order prevents resource leaks on long runs.</item>
///   <item><see cref="IPage"/> and <see cref="IBrowserContext"/> are scoped
///         per test and managed entirely by <c>BaseTest</c>.</item>
/// </list>
/// </remarks>
public static class DriverManager
{
    private static IPlaywright? _playwright;
    private static IBrowser? _browser;

    /// <summary>
    /// Creates the <see cref="IPlaywright"/> runtime and launches the shared
    /// browser configured in <see cref="ConfigManager"/>.
    /// Safe to call multiple times — subsequent calls within the same run are
    /// no-ops if the browser is already running.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the configured browser name does not map to a known
    /// <see cref="BrowserType"/> value.
    /// </exception>
    public static async Task InitializeBrowserAsync()
    {
        if (_browser is not null)
            return;

        var settings = ConfigManager.Instance.Settings;

        if (!Enum.TryParse<BrowserType>(settings.Browser.Browser, ignoreCase: true, out var browserType))
            throw new InvalidOperationException(
                $"'{settings.Browser.Browser}' is not a supported browser type. " +
                $"Valid values: {string.Join(", ", Enum.GetNames<BrowserType>())}");

        _playwright = await Playwright.CreateAsync();
        _browser    = await DriverFactory.CreateBrowserAsync(_playwright, browserType, settings.Browser.Headless);
    }

    /// <summary>
    /// Creates a new isolated browser context and page for a single test.
    /// Each call returns a fresh <see cref="IPage"/> with no shared state.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when called before <see cref="InitializeBrowserAsync"/>.
    /// </exception>
    public static async Task<IPage> CreatePageAsync()
    {
        if (_browser is null)
            throw new InvalidOperationException(
                "Browser has not been initialized. " +
                "Call InitializeBrowserAsync() before creating pages.");

        var context = await _browser.NewContextAsync();
        return await context.NewPageAsync();
    }

    /// <summary>
    /// Disposes the page and its parent browser context.
    /// Null-safe — passing a null page is silently ignored.
    /// </summary>
    public static async Task ClosePageAsync(IPage? page)
    {
        if (page is null)
            return;

        try
        {
            // Disposing the context also closes the page cleanly.
            await page.Context.DisposeAsync();
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "[DriverManager] Browser context disposal failed — continuing teardown");
        }
    }

    /// <summary>
    /// Disposes the shared browser and the <see cref="IPlaywright"/> runtime.
    /// Disposal order is browser first, then Playwright — reversing creation
    /// order to avoid resource leaks.
    /// Safe to call when neither has been initialized (unit-only runs).
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
            Log.Warning(ex, "[DriverManager] Browser disposal failed — continuing cleanup");
        }
        finally
        {
            _browser = null;
        }

        try
        {
            // Playwright must be disposed after the browser — it owns the
            // underlying browser process handles.
            _playwright?.Dispose();
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "[DriverManager] Playwright disposal failed — continuing cleanup");
        }
        finally
        {
            _playwright = null;
        }
    }
}
