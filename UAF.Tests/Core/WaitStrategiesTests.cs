using FluentAssertions;
using Microsoft.Playwright;
using NUnit.Framework;
using UAF.Core.Utils;

namespace UAF.Tests.Core;

/// <summary>
/// Integration tests for <see cref="WaitStrategies"/>.
/// Each test uses a real Playwright page with inline HTML or a known URL so
/// there is no dependency on any external service.
/// </summary>
/// <remarks>
/// All tests are tagged <c>Integration</c> and are excluded from the standard
/// CI pipeline via <c>--filter "TestCategory!=Integration"</c>.
/// This class manages its own browser lifecycle rather than inheriting from
/// <c>BaseTest</c> to keep the Playwright setup explicit and self-contained.
/// </remarks>
[TestFixture]
[Category("Integration")]
public class WaitStrategiesTests
{
    private IPlaywright _playwright = null!;
    private IBrowser _browser = null!;
    private IPage _page = null!;

    private const string TestHtml = "<html><body>" +
        "<div id='target' style='display:none'>Hello</div>" +
        "<button id='show-btn' onclick=\"" +
            "setTimeout(() => document.getElementById('target').style.display='block', 300)" +
        "\">Show</button>" +
        "</body></html>";

    /// <summary>
    /// Launches a headless Chromium instance shared across all tests in this fixture.
    /// </summary>
    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true
        });
    }

    /// <summary>Creates a fresh isolated page before each test.</summary>
    [SetUp]
    public async Task SetUp()
    {
        _page = await _browser.NewPageAsync();
    }

    /// <summary>Closes the page after each test.</summary>
    [TearDown]
    public async Task TearDown()
    {
        await _page.CloseAsync();
    }

    /// <summary>Disposes the browser and Playwright instance after all tests complete.</summary>
    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await _browser.CloseAsync();
        _playwright.Dispose();
    }

    /// <summary>
    /// Verifies that <see cref="WaitStrategies.WaitForSelectorAsync"/> completes
    /// without error when the target element becomes visible within the timeout.
    /// </summary>
    [Test]
    public async Task Should_Complete_WhenSelectorBecomesVisible()
    {
        await _page.SetContentAsync(TestHtml);
        await _page.ClickAsync("#show-btn");

        var act = async () => await WaitStrategies.WaitForSelectorAsync(_page, "#target");

        await act.Should().NotThrowAsync();
    }

    /// <summary>
    /// Verifies that <see cref="WaitStrategies.WaitForSelectorAsync"/> throws a
    /// <see cref="PlaywrightException"/> when the selector never appears within the timeout.
    /// </summary>
    [Test]
    public async Task Should_ThrowPlaywrightException_WhenSelectorNeverAppears()
    {
        await _page.SetContentAsync(TestHtml);

        var act = async () => await WaitStrategies.WaitForSelectorAsync(_page, "#does-not-exist", timeoutMs: 1000);

        await act.Should().ThrowAsync<PlaywrightException>();
    }

    /// <summary>
    /// Verifies that <see cref="WaitStrategies.WaitForUrlAsync"/> completes without
    /// error when the current page URL matches the supplied pattern.
    /// </summary>
    [Test]
    public async Task Should_Complete_WhenUrlContainsPattern()
    {
        await _page.GotoAsync("about:blank");

        var act = async () => await WaitStrategies.WaitForUrlAsync(_page, "about:blank");

        await act.Should().NotThrowAsync();
    }

    /// <summary>
    /// Verifies that <see cref="WaitStrategies.WaitForNetworkIdleAsync"/> completes
    /// without error when no network activity is occurring.
    /// </summary>
    [Test]
    public async Task Should_Complete_WhenNetworkIsIdle()
    {
        await _page.SetContentAsync(TestHtml);

        var act = async () => await WaitStrategies.WaitForNetworkIdleAsync(_page);

        await act.Should().NotThrowAsync();
    }
}