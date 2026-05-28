using FluentAssertions;
using Microsoft.Playwright;
using UAF.Core.Driver;
using BrowserType = UAF.Core.Driver.BrowserType;

namespace UAF.Tests.Unit;

/// <summary>
/// Tests for <see cref="DriverFactory"/>.
/// Integration tests require a real Playwright instance and a browser binary —
/// they are tagged <c>[Category("Integration")]</c> and filtered out of the
/// unit-only CI stage.
/// <see cref="IPlaywright"/> is created once per fixture and disposed in
/// <c>OneTimeTearDown</c>, mirroring the ownership model used by
/// <see cref="DriverManager"/> in production.
/// </summary>
[TestFixture]
public class DriverFactoryTests
{
    private IPlaywright _playwright = null!;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        _playwright = await Playwright.CreateAsync();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        // Run on a background thread to avoid potential sync-context issues
        // with Playwright's dispose path inside NUnit's teardown runner.
        await Task.Run(() => _playwright.Dispose());
    }

    [Test]
    [Category("Integration")]
    public async Task Should_ThrowArgumentOutOfRangeException_WhenUnsupportedBrowserTypeProvided()
    {
        var act = async () => await DriverFactory.CreateBrowserAsync(_playwright, (BrowserType)99, headless: true);

        await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

    [Test]
    [Category("Integration")]
    public async Task Should_CreateChromiumBrowser_WhenBrowserTypeIsChromium()
    {
        var browser = await DriverFactory.CreateBrowserAsync(_playwright, BrowserType.Chromium, headless: true);
        browser.Should().NotBeNull();
        await browser.DisposeAsync();
    }

    [Test]
    [Category("Integration")]
    public async Task Should_CreateFirefoxBrowser_WhenBrowserTypeIsFirefox()
    {
        var browser = await DriverFactory.CreateBrowserAsync(_playwright, BrowserType.Firefox, headless: true);
        browser.Should().NotBeNull();
        await browser.DisposeAsync();
    }

    [Test]
    [Category("Integration")]
    public async Task Should_CreateWebKitBrowser_WhenBrowserTypeIsWebKit()
    {
        var browser = await DriverFactory.CreateBrowserAsync(_playwright, BrowserType.WebKit, headless: true);
        browser.Should().NotBeNull();
        await browser.DisposeAsync();
    }

    [Test]
    [Category("Integration")]
    public async Task Should_CreateHeadlessBrowser_WhenHeadlessIsTrue()
    {
        var browser = await DriverFactory.CreateBrowserAsync(_playwright, BrowserType.Chromium, headless: true);
        browser.Should().NotBeNull();
        browser.IsConnected.Should().BeTrue();
        await browser.DisposeAsync();
    }

    [Test]
    [Category("Integration")]
    public async Task Should_ReturnIBrowser_WhenBrowserCreatedSuccessfully()
    {
        var browser = await DriverFactory.CreateBrowserAsync(_playwright, BrowserType.Chromium, headless: true);
        browser.Should().BeAssignableTo<IBrowser>();
        await browser.DisposeAsync();
    }
}
