using FluentAssertions;
using Microsoft.Playwright;
using NUnit.Framework;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.InMemory;
using UAF.Core.Base;

namespace UAF.Tests.Unit.Core.Base;

/// <summary>
/// Integration tests verifying that <see cref="BasePage"/> emits
/// <c>Debug</c>-level log entries for each public method, and that those
/// entries are suppressed at the default <c>Information</c> level.
/// </summary>
/// <remarks>
/// Tests are tagged <c>Integration</c> because they require a real Playwright
/// page to invoke the underlying Playwright operations after the log call.
/// Each test configures its own in-memory Serilog sink so assertions are fully
/// isolated from the assembly-level logger configured by
/// <c>AssemblySetupFixture</c>.
/// </remarks>
[TestFixture]
[Category("Integration")]
public class BasePageDebugLoggingTests
{
    private IPlaywright _playwright = null!;
    private IBrowser _browser = null!;
    private IPage _page = null!;
    private TestPage _sut = null!;
    private InMemorySink _sink = null!;

    private const string TestHtml =
        "<button id='btn'>Click</button>" +
        "<input  id='inp' />" +
        "<p      id='txt'>Hello</p>";

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
    }

    [SetUp]
    public async Task SetUp()
    {
        var context = await _browser.NewContextAsync();
        _page = await context.NewPageAsync();
        _sut  = new TestPage(_page);
        await _page.SetContentAsync(TestHtml);
    }

    [TearDown]
    public async Task TearDown()
    {
        await _page.Context.DisposeAsync();
        Log.CloseAndFlush();
        _sink?.Dispose();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await _browser.DisposeAsync();
        _playwright.Dispose();
    }

    // ── Helper ──────────────────────────────────────────────────────────────

    private void ConfigureLogger(LogEventLevel minimumLevel)
    {
        _sink = new InMemorySink();
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Is(minimumLevel)
            .WriteTo.Sink(_sink)
            .CreateLogger();
    }

    // ── Debug-level tests ───────────────────────────────────────────────────

    /// <summary>
    /// Verifies that <see cref="BasePage.ClickAsync"/> emits a <c>Debug</c>
    /// log entry containing the method name and selector.
    /// </summary>
    [Test]
    public async Task Should_EmitDebugEvent_WhenClickAsyncCalled()
    {
        ConfigureLogger(LogEventLevel.Debug);

        await _sut.DoClickAsync("#btn");

        _sink.LogEvents
            .Any(e => e.Level == LogEventLevel.Debug && e.RenderMessage().Contains("[BasePage] ClickAsync '#btn'"))
            .Should().BeTrue("a Debug event for ClickAsync '#btn' should have been emitted");
    }

    /// <summary>
    /// Verifies that <see cref="BasePage.FillAsync"/> emits a <c>Debug</c>
    /// log entry containing the method name and selector (not the value,
    /// which may be sensitive).
    /// </summary>
    [Test]
    public async Task Should_EmitDebugEvent_WhenFillAsyncCalled()
    {
        ConfigureLogger(LogEventLevel.Debug);

        await _sut.DoFillAsync("#inp", "test-value");

        _sink.LogEvents
            .Any(e => e.Level == LogEventLevel.Debug && e.RenderMessage().Contains("[BasePage] FillAsync '#inp'"))
            .Should().BeTrue("a Debug event for FillAsync '#inp' should have been emitted");
    }

    /// <summary>
    /// Verifies that <see cref="BasePage.IsVisibleAsync"/> emits a <c>Debug</c>
    /// log entry containing the method name and selector.
    /// </summary>
    [Test]
    public async Task Should_EmitDebugEvent_WhenIsVisibleAsyncCalled()
    {
        ConfigureLogger(LogEventLevel.Debug);

        await _sut.DoIsVisibleAsync("#txt");

        _sink.LogEvents
            .Any(e => e.Level == LogEventLevel.Debug && e.RenderMessage().Contains("[BasePage] IsVisibleAsync '#txt'"))
            .Should().BeTrue("a Debug event for IsVisibleAsync '#txt' should have been emitted");
    }

    /// <summary>
    /// Verifies that <see cref="BasePage.TakeScreenshotAsync"/> emits a
    /// <c>Debug</c> log entry containing the method name (no selector parameter).
    /// </summary>
    [Test]
    public async Task Should_EmitDebugEvent_WhenTakeScreenshotAsyncCalled()
    {
        ConfigureLogger(LogEventLevel.Debug);

        await _sut.DoTakeScreenshotAsync();

        _sink.LogEvents
            .Any(e => e.Level == LogEventLevel.Debug && e.RenderMessage().Contains("[BasePage] TakeScreenshotAsync"))
            .Should().BeTrue("a Debug event for TakeScreenshotAsync should have been emitted");
    }

    // ── Information-level silence test ──────────────────────────────────────

    /// <summary>
    /// Verifies that no <c>Debug</c> log entries are produced when the logger
    /// is configured at <c>Information</c> level — the default for all runs.
    /// </summary>
    [Test]
    public async Task Should_ProduceNoDebugEvents_WhenLoggerIsAtInformationLevel()
    {
        ConfigureLogger(LogEventLevel.Information);

        await _sut.DoClickAsync("#btn");
        await _sut.DoFillAsync("#inp", "value");
        await _sut.DoIsVisibleAsync("#txt");

        _sink.LogEvents.Any(e => e.Level == LogEventLevel.Debug)
            .Should().BeFalse("no Debug events should be emitted when the logger minimum level is Information");
    }

    // ── Inline test double ───────────────────────────────────────────────────

    /// <summary>
    /// Minimal concrete subclass that promotes the protected
    /// <see cref="BasePage"/> members to public visibility for testing.
    /// </summary>
    private sealed class TestPage : BasePage
    {
        public TestPage(IPage page) : base(page) { }

        public Task DoClickAsync(string selector)           => ClickAsync(selector);
        public Task DoFillAsync(string selector, string v)  => FillAsync(selector, v);
        public Task<bool> DoIsVisibleAsync(string selector) => IsVisibleAsync(selector);
        public Task<byte[]> DoTakeScreenshotAsync()         => TakeScreenshotAsync();
    }
}