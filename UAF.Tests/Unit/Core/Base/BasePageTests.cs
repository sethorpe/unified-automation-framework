using FluentAssertions;
using Microsoft.Playwright;
using UAF.Core.Base;
using UAF.Core.Driver;

namespace UAF.Tests.Unit.Core.Base;

/// <summary>
/// Integration tests for <see cref="BasePage"/>.
/// A minimal concrete subclass (<see cref="TestPage"/>) is defined inline to
/// satisfy the abstract constraint without pulling in any real page object.
/// All tests use a <c>data:</c> URI so there is no dependency on any external
/// site or network connection.
/// </summary>
[TestFixture]
[Category("Integration")]
public class BasePageTests
{
    // ── Inline test double ──────────────────────────────────────────────────

    /// <summary>
    /// Minimal concrete subclass that promotes the protected BasePage members
    /// to internal visibility so the test class can call them directly.
    /// </summary>
    private sealed class TestPage : BasePage
    {
        public TestPage(IPage page) : base(page) { }

        // Expose protected members for test assertions
        public Task DoClickAsync(string selector)           => ClickAsync(selector);
        public Task DoFillAsync(string selector, string v)  => FillAsync(selector, v);
        public Task<string> DoGetTextAsync(string selector) => GetTextAsync(selector);
        public Task<bool> DoIsVisibleAsync(string selector) => IsVisibleAsync(selector);
        public Task DoWaitForSelectorAsync(string selector) => WaitForSelectorAsync(selector);
        public Task<byte[]> DoTakeScreenshotAsync()         => TakeScreenshotAsync();
        public string DoPageUrl                             => PageUrl;
    }

    // ── Fixtures ────────────────────────────────────────────────────────────

    private IPlaywright _playwright = null!;
    private IBrowser _browser = null!;
    private IPage _page = null!;
    private TestPage _sut = null!;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        _playwright = await Playwright.CreateAsync();
        _browser    = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
    }

    [SetUp]
    public async Task SetUp()
    {
        var context = await _browser.NewContextAsync();
        _page = await context.NewPageAsync();
        _sut  = new TestPage(_page);

        // A self-contained HTML page served inline — no external network calls.
        await _page.GotoAsync(
            "data:text/html," +
            "<button id='btn'>Click Me</button>" +
            "<input  id='inp' />" +
            "<p      id='txt'>Hello World</p>" +
            "<span   id='vis'>Visible</span>");
    }

    [TearDown]
    public async Task TearDown()
    {
        await _page.Context.DisposeAsync();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await _browser.DisposeAsync();
        _playwright.Dispose();
    }

    // ── Tests ───────────────────────────────────────────────────────────────

    [Test]
    public async Task ClickAsync_ShouldActivateTargetElement()
    {
        // Arrange — attach a click listener that sets a data attribute we can assert on
        await _page.EvaluateAsync(
            "document.getElementById('btn').addEventListener('click', () => " +
            "{ document.getElementById('btn').setAttribute('data-clicked', 'true'); })");

        // Act
        await _sut.DoClickAsync("#btn");

        // Assert
        var clicked = await _page.GetAttributeAsync("#btn", "data-clicked");
        clicked.Should().Be("true");
    }

    [Test]
    public async Task FillAsync_ShouldPopulateInputWithGivenValue()
    {
        // Act
        await _sut.DoFillAsync("#inp", "hello");

        // Assert
        var value = await _page.InputValueAsync("#inp");
        value.Should().Be("hello");
    }

    [Test]
    public async Task GetTextAsync_ShouldReturnVisibleTextOfElement()
    {
        // Act
        var text = await _sut.DoGetTextAsync("#txt");

        // Assert
        text.Should().Be("Hello World");
    }

    [Test]
    public async Task IsVisibleAsync_ShouldReturnTrue_WhenElementIsPresent()
    {
        // Act
        var visible = await _sut.DoIsVisibleAsync("#vis");

        // Assert
        visible.Should().BeTrue();
    }

    [Test]
    public async Task IsVisibleAsync_ShouldReturnFalse_WhenElementIsAbsent()
    {
        // Act — selector that does not exist in the DOM
        var visible = await _sut.DoIsVisibleAsync("#does-not-exist");

        // Assert
        visible.Should().BeFalse();
    }

    [Test]
    public async Task WaitForSelectorAsync_ShouldComplete_WhenElementAppears()
    {
        // Arrange — inject a paragraph after a short delay via JS
        await _page.EvaluateAsync(
            "setTimeout(() => {" +
            "  const el = document.createElement('p');" +
            "  el.id = 'late';" +
            "  el.textContent = 'Late element';" +
            "  document.body.appendChild(el);" +
            "}, 200)");

        // Act & Assert — should not throw; the element arrives within the default timeout
        var act = async () => await _sut.DoWaitForSelectorAsync("#late");
        await act.Should().NotThrowAsync();
    }

    [Test]
    public async Task TakeScreenshotAsync_ShouldReturnNonEmptyByteArray()
    {
        // Act
        var screenshot = await _sut.DoTakeScreenshotAsync();

        // Assert
        screenshot.Should().NotBeNullOrEmpty();
    }

    [Test]
    public void PageUrl_ShouldReturnCurrentPageUrl()
    {
        // Act — PageUrl is a synchronous property; no async needed
        var url = _sut.DoPageUrl;

        // Assert — data URIs start with "data:"
        url.Should().StartWith("data:");
    }
}
