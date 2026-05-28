using Microsoft.Playwright;

namespace UAF.Core.Driver;

/// <summary>
/// Translates a <see cref="BrowserType"/> value into a running
/// <see cref="IBrowser"/> instance.
/// </summary>
/// <remarks>
/// <see cref="IPlaywright"/> is accepted as a parameter rather than created
/// internally so that the caller (<see cref="DriverManager"/>) owns the full
/// Playwright lifecycle and can dispose it correctly when the run ends.
/// </remarks>
public static class DriverFactory
{
    /// <summary>
    /// Launches a browser of the requested type using the supplied
    /// <see cref="IPlaywright"/> instance.
    /// </summary>
    /// <param name="playwright">
    /// The active <see cref="IPlaywright"/> instance. Owned and disposed by
    /// the caller — <see cref="DriverFactory"/> never disposes it.
    /// </param>
    /// <param name="browserType">
    /// The browser engine to launch.
    /// </param>
    /// <param name="headless">
    /// <c>true</c> to run without a visible window (default for CI);
    /// <c>false</c> for local debugging.
    /// </param>
    /// <returns>A running <see cref="IBrowser"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="browserType"/> is not a recognised value.
    /// There is no silent fallback — an unsupported type is always an error.
    /// </exception>
    public static async Task<IBrowser> CreateBrowserAsync(
        IPlaywright playwright,
        BrowserType browserType,
        bool headless)
    {
        var launchOptions = new BrowserTypeLaunchOptions { Headless = headless };

        return browserType switch
        {
            BrowserType.Chromium => await playwright.Chromium.LaunchAsync(launchOptions),
            BrowserType.Firefox  => await playwright.Firefox.LaunchAsync(launchOptions),
            BrowserType.WebKit   => await playwright.Webkit.LaunchAsync(launchOptions),
            _ => throw new ArgumentOutOfRangeException(
                     nameof(browserType),
                     $"Unsupported browser type: {browserType}")
        };
    }
}
