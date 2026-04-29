using Microsoft.Playwright;

namespace UAF.Core.Driver;

public static class DriverFactory
{
    public static async Task<IBrowser> CreateBrowserAsync(
        BrowserType browserType,
        bool headless)
    {
        var playwright = await Playwright.CreateAsync();

        var launchOptions = new BrowserTypeLaunchOptions
        {
            Headless = headless
        };

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
