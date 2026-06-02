using FluentAssertions;
using NUnit.Framework;
using UAF.Core.Driver;
using UAF.UI.Pages;
using Microsoft.Playwright;

namespace UAF.Tests.UI;

/// <summary>
/// Integration tests for <see cref="LoginPage"/> against the live SauceDemo site.
/// Requires a real browser and a network connection — excluded from CI via
/// <c>--filter "TestCategory!=Integration"</c>.
/// </summary>
[TestFixture]
[Category("Integration")]
public class LoginPageTests
{
    private IPage _page = null!;
    private LoginPage _loginPage = null!;

    /// <summary>Launches the shared browser before any test in this fixture runs.</summary>
    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        await DriverManager.InitializeBrowserAsync();
    }

    /// <summary>Creates a fresh isolated page and a new <see cref="LoginPage"/> before each test.</summary>
    [SetUp]
    public async Task SetUp()
    {
        _page = await DriverManager.CreatePageAsync();
        _loginPage = new LoginPage(_page);
    }

    /// <summary>Closes the page after each test.</summary>
    [TearDown]
    public async Task TearDown()
    {
        await DriverManager.ClosePageAsync(_page);
    }

    /// <summary>Disposes the shared browser after all tests in this fixture complete.</summary>
    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await DriverManager.DisposeBrowserAsync();
    }

    /// <summary>
    /// Verifies that <see cref="LoginPage.OpenAsync"/> navigates to the SauceDemo login page.
    /// </summary>
    [Test]
    public async Task Should_NavigateToLoginPage_WhenOpenIsCalled()
    {
        await _loginPage.OpenAsync();

        _page.Url.Should().Contain("saucedemo.com");
    }

    /// <summary>
    /// Verifies that <see cref="LoginPage.LoginAsync"/> returns an
    /// <see cref="InventoryPage"/> instance when valid credentials are supplied.
    /// </summary>
    [Test]
    public async Task Should_ReturnInventoryPage_WhenValidCredentialsProvided()
    {
        await _loginPage.OpenAsync();

        var result = await _loginPage.LoginAsync("standard_user", "secret_sauce");

        result.Should().NotBeNull().And.BeOfType<InventoryPage>();
    }

    /// <summary>
    /// Verifies that <see cref="LoginPage.GetErrorMessageAsync"/> returns a
    /// non-empty string when invalid credentials are submitted.
    /// </summary>
    [Test]
    public async Task Should_DisplayErrorMessage_WhenInvalidCredentialsProvided()
    {
        await _loginPage.OpenAsync();
        await _loginPage.LoginAsync("invalid_user", "wrong_password");

        var error = await _loginPage.GetErrorMessageAsync();

        error.Should().NotBeEmpty();
    }

    /// <summary>
    /// Verifies that <see cref="LoginPage.GetErrorMessageAsync"/> returns an
    /// empty string when no login has been attempted and no error is present.
    /// </summary>
    [Test]
    public async Task Should_ReturnEmptyString_WhenNoErrorMessagePresent()
    {
        await _loginPage.OpenAsync();

        var error = await _loginPage.GetErrorMessageAsync();

        error.Should().BeEmpty();
    }
}
