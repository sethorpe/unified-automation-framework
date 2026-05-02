using FluentAssertions;
using Microsoft.Playwright;
using UAF.Core.Driver;

namespace UAF.Tests.Unit;

[TestFixture]
public class DriverManagerTests
{
    // --- Unit tests ---

    /// <summary>
    /// AssemblySetupFixture initializes the browser before any test runs,
    /// so this test must temporarily reset shared state to exercise the guard.
    /// The finally block restores the browser so subsequent tests are unaffected.
    /// </summary>
    [Test]
    [Category("Unit")]
    public async Task Should_ThrowInvalidOperationException_WhenCreatePageCalledBeforeInitialize()
    {
        await DriverManager.DisposeBrowserAsync();

        try
        {
            var act = async () => await DriverManager.CreatePageAsync();

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*AssemblySetupFixture*");
        }
        finally
        {
            await DriverManager.InitializeBrowserAsync();
        }
    }

    [Test]
    [Category("Unit")]
    public async Task Should_NotThrow_WhenClosePageCalledWithNullPage()
    {
        var act = async () => await DriverManager.ClosePageAsync(null);

        await act.Should().NotThrowAsync();
    }

    // --- Integration tests (real browser) ---

    [Test]
    [Category("Integration")]
    public async Task Should_ReturnNewPage_WhenCreatePageAsyncCalled()
    {
        var page = await DriverManager.CreatePageAsync();

        page.Should().NotBeNull();

        await DriverManager.ClosePageAsync(page);
    }

    [Test]
    [Category("Integration")]
    public async Task Should_ReturnDistinctPages_WhenCreatePageCalledTwice()
    {
        var page1 = await DriverManager.CreatePageAsync();
        var page2 = await DriverManager.CreatePageAsync();

        page1.Should().NotBeSameAs(page2);

        await DriverManager.ClosePageAsync(page1);
        await DriverManager.ClosePageAsync(page2);
    }
}
