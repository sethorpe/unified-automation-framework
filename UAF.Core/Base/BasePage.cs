using Microsoft.Playwright;

namespace UAF.Core.Base;

/// <summary>
/// Abstract base class for all Playwright page objects.
/// Wraps the most common <see cref="IPage"/> interactions so that concrete
/// page objects never touch <see cref="IPage"/> directly, keeping UI mechanics
/// in one place and pages focused on their own vocabulary.
/// </summary>
/// <remarks>
/// Lifecycle contract:
/// <list type="bullet">
///   <item>The caller (BaseTest) owns the <see cref="IPage"/> instance and is
///         responsible for creating and disposing it.</item>
///   <item>All timeouts default to <c>null</c>, which defers to whatever
///         default timeout is configured on the browser context — allowing
///         the <c>ConfigManager</c> timeout setting to flow through naturally
///         without any extra wiring here.</item>
///   <item>No logging, no Allure steps, no assertions live here — those
///         belong in the test layer.</item>
/// </list>
/// </remarks>
public abstract class BasePage
{
    /// <summary>
    /// The underlying Playwright page driving this page object.
    /// Exposed as <c>protected</c> so concrete page objects can call
    /// Playwright APIs that are not wrapped here (e.g. <c>GotoAsync</c>).
    /// </summary>
    protected IPage Page { get; }

    /// <summary>
    /// The current URL of the page at the moment the property is read.
    /// Delegates to <see cref="IPage.Url"/> — no network round-trip required.
    /// </summary>
    protected string PageUrl => Page.Url;

    /// <summary>
    /// Initializes a new instance of <see cref="BasePage"/> with the
    /// Playwright page that drives this page object.
    /// </summary>
    /// <param name="page">
    /// The active Playwright <see cref="IPage"/>. Must not be null.
    /// </param>
    protected BasePage(IPage page)
    {
        Page = page ?? throw new ArgumentNullException(nameof(page));
    }

    /// <summary>
    /// Clicks the first element matching <paramref name="selector"/>.
    /// Waits for the element to be visible and enabled before clicking
    /// (Playwright auto-waiting — no <c>Thread.Sleep</c> required).
    /// </summary>
    /// <param name="selector">A CSS, XPath, or Playwright text selector.</param>
    protected async Task ClickAsync(string selector)
    {
        await Page.ClickAsync(selector);
    }

    /// <summary>
    /// Clears the input matching <paramref name="selector"/> and types
    /// <paramref name="value"/> into it.
    /// Waits for the element to be editable before filling
    /// (Playwright auto-waiting — no <c>Thread.Sleep</c> required).
    /// </summary>
    /// <param name="selector">A CSS, XPath, or Playwright text selector.</param>
    /// <param name="value">The text to fill into the element.</param>
    protected async Task FillAsync(string selector, string value)
    {
        await Page.FillAsync(selector, value);
    }

    /// <summary>
    /// Returns the visible inner text of the first element matching
    /// <paramref name="selector"/>.
    /// </summary>
    /// <param name="selector">A CSS, XPath, or Playwright text selector.</param>
    /// <returns>The trimmed inner text of the matched element.</returns>
    protected async Task<string> GetTextAsync(string selector)
    {
        return await Page.InnerTextAsync(selector);
    }

    /// <summary>
    /// Returns <c>true</c> when the first element matching
    /// <paramref name="selector"/> is visible in the viewport;
    /// <c>false</c> otherwise.
    /// Does not wait — evaluates the current DOM state immediately.
    /// </summary>
    /// <param name="selector">A CSS, XPath, or Playwright text selector.</param>
    protected async Task<bool> IsVisibleAsync(string selector)
    {
        return await Page.IsVisibleAsync(selector);
    }

    /// <summary>
    /// Waits until the element matching <paramref name="selector"/> appears
    /// in the DOM and is visible.
    /// Uses Playwright's built-in waiting — no polling or <c>Thread.Sleep</c>.
    /// </summary>
    /// <param name="selector">A CSS, XPath, or Playwright text selector.</param>
    protected async Task WaitForSelectorAsync(string selector)
    {
        await Page.WaitForSelectorAsync(selector);
    }

    /// <summary>
    /// Waits until the page URL matches <paramref name="urlOrPattern"/>.
    /// Accepts an exact URL string, a glob pattern, or a regex string.
    /// Useful for confirming navigation has completed after a click or form submit.
    /// </summary>
    /// <param name="urlOrPattern">
    /// An exact URL, glob (e.g. <c>"**/dashboard"</c>), or regex pattern.
    /// </param>
    protected async Task WaitForUrlAsync(string urlOrPattern)
    {
        await Page.WaitForURLAsync(urlOrPattern);
    }

    /// <summary>
    /// Captures a full-page screenshot of the current browser viewport.
    /// </summary>
    /// <returns>
    /// A byte array containing the PNG-encoded screenshot.
    /// Never empty when the page is open and rendering.
    /// </returns>
    protected async Task<byte[]> TakeScreenshotAsync()
    {
        return await Page.ScreenshotAsync(new PageScreenshotOptions
        {
            FullPage = true
        });
    }
}
