using FluentAssertions;
using Microsoft.Playwright;
using UAF.Core.Driver;

namespace UAF.Tests.Unit;

[TestFixture]
public class DriverManagerUnitTests
{
    [Test]
    [Category("Unit")]
    public async Task Should_NotThrow_WhenClosePageCalledWithNullPage()
    {
        var act = async () => await DriverManager.ClosePageAsync(null);

        await act.Should().NotThrowAsync();
    }
}

[TestFixture]
public class DriverManagerIntegrationTests
{
    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        await DriverManager.InitializeBrowserAsync();
    }

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
