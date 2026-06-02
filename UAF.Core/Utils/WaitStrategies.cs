using Microsoft.Playwright;
using UAF.Core.Config;

namespace UAF.Core.Utils;

/// <summary>
/// Static helpers that wrap Playwright's built-in waiting API with named,
/// explicit wait operations. Use these when Playwright's implicit auto-waiting
/// is not sufficient — for example, when a test must wait for a hidden element
/// to appear, a URL change to complete, or the network to go idle.
/// </summary>
/// <remarks>
/// All methods default their timeout to <see cref="BrowserSettings.Timeout"/>
/// from <see cref="ConfigManager"/> when <c>timeoutMs</c> is not supplied.
/// No <c>Thread.Sleep</c> is used — every wait delegates to Playwright.
/// </remarks>
public static class WaitStrategies
{
    /// <summary>
    /// Waits until the element matching <paramref name="selector"/> is present
    /// in the DOM and visible in the viewport
    /// (<see cref="WaitForSelectorState.Visible"/>).
    /// </summary>
    /// <param name="page">The Playwright page to wait on.</param>
    /// <param name="selector">A CSS, XPath, or Playwright text selector.</param>
    /// <param name="timeoutMs">
    /// Maximum time to wait in milliseconds. Defaults to
    /// <see cref="BrowserSettings.Timeout"/> when <c>null</c>.
    /// </param>
    public static async Task WaitForSelectorAsync(IPage page, string selector, int? timeoutMs = null)
    {
        var timeout = timeoutMs ?? ConfigManager.Instance.Settings.Browser.Timeout;
        await page.WaitForSelectorAsync(selector, new PageWaitForSelectorOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = (float?)timeout
        });
    }

    /// <summary>
    /// Waits until the element matching <paramref name="selector"/> is hidden
    /// or detached from the DOM
    /// (<see cref="WaitForSelectorState.Hidden"/>).
    /// </summary>
    /// <param name="page">The Playwright page to wait on.</param>
    /// <param name="selector">A CSS, XPath, or Playwright text selector.</param>
    /// <param name="timeoutMs">
    /// Maximum time to wait in milliseconds. Defaults to
    /// <see cref="BrowserSettings.Timeout"/> when <c>null</c>.
    /// </param>
    public static async Task WaitForSelectorHiddenAsync(IPage page, string selector, int? timeoutMs = null)
    {
        var timeout = timeoutMs ?? ConfigManager.Instance.Settings.Browser.Timeout;
        await page.WaitForSelectorAsync(selector, new PageWaitForSelectorOptions
        {
            State = WaitForSelectorState.Hidden,
            Timeout = (float?)timeout
        });
    }

    /// <summary>
    /// Waits until the page URL matches <paramref name="urlPattern"/>.
    /// Accepts an exact URL string, a glob pattern (e.g. <c>**dashboard**</c>),
    /// or a regex string.
    /// </summary>
    /// <param name="page">The Playwright page to wait on.</param>
    /// <param name="urlPattern">
    /// An exact URL, glob pattern, or regex pattern to match against the current URL.
    /// </param>
    /// <param name="timeoutMs">
    /// Maximum time to wait in milliseconds. Defaults to
    /// <see cref="BrowserSettings.Timeout"/> when <c>null</c>.
    /// </param>
    public static async Task WaitForUrlAsync(IPage page, string urlPattern, int? timeoutMs = null)
    {
        var timeout = timeoutMs ?? ConfigManager.Instance.Settings.Browser.Timeout;
        await page.WaitForURLAsync(urlPattern, new PageWaitForURLOptions
        {
            Timeout = (float?)timeout
        });
    }

    /// <summary>
    /// Waits until there are no active network connections for at least 500 ms
    /// (<see cref="LoadState.Networkidle"/>). Useful after triggering navigation
    /// or actions that fire background requests.
    /// </summary>
    /// <param name="page">The Playwright page to wait on.</param>
    /// <param name="timeoutMs">
    /// Maximum time to wait in milliseconds. Defaults to
    /// <see cref="BrowserSettings.Timeout"/> when <c>null</c>.
    /// </param>
    public static async Task WaitForNetworkIdleAsync(IPage page, int? timeoutMs = null)
    {
        var timeout = timeoutMs ?? ConfigManager.Instance.Settings.Browser.Timeout;
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle, new PageWaitForLoadStateOptions
        {
            Timeout = (float?)timeout
        });
    }
}