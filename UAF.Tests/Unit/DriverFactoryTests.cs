using FluentAssertions;
using Microsoft.Playwright;
using UAF.Core.Driver;
using BrowserType = UAF.Core.Driver.BrowserType;

namespace UAF.Tests.Unit;

[TestFixture]
public class DriverFactoryTests
{
    [Test]
    [Category("Unit")]
    public async Task Should_ThrowArgumentOutOfRangeException_WhenUnsupportedBrowserTypeProvided()
    {
        var act = async () => await DriverFactory.CreateBrowserAsync((BrowserType)99, headless: true);
        await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

    [Test]
    [Category("Integration")]
    public async Task Should_CreateChromiumBrowser_WhenBrowserTypeIsChromium()
    {
        var browser = await DriverFactory.CreateBrowserAsync(BrowserType.Chromium, headless: true);
        browser.Should().NotBeNull();
        await browser.DisposeAsync();
    }

    [Test]
    [Category("Integration")]
    public async Task Should_CreateFirefoxBrowser_WhenBrowserTypeIsFirefox()
    {
        var browser = await DriverFactory.CreateBrowserAsync(BrowserType.Firefox, headless: true);
        browser.Should().NotBeNull();
        await browser.DisposeAsync();
    }

    [Test]
    [Category("Integration")]
    public async Task Should_CreateWebKitBrowser_WhenBrowserTypeIsWebKit()
    {
        var browser = await DriverFactory.CreateBrowserAsync(BrowserType.WebKit, headless: true);
        browser.Should().NotBeNull();
        await browser.DisposeAsync();
    }

    [Test]
    [Category("Integration")]
    public async Task Should_CreateHeadlessBrowser_WhenHeadlessIsTrue()
    {
        var browser = await DriverFactory.CreateBrowserAsync(BrowserType.Chromium, headless: true);
        browser.Should().NotBeNull();
        browser.IsConnected.Should().BeTrue();
        await browser.DisposeAsync();
    }

    [Test]
    [Category("Integration")]
    public async Task Should_ReturnIBrowser_WhenBrowserCreatedSuccessfully()
    {
        var browser = await DriverFactory.CreateBrowserAsync(BrowserType.Chromium, headless: true);
        browser.Should().BeAssignableTo<IBrowser>();
        await browser.DisposeAsync();
    }
}
