using Microsoft.Playwright;

namespace UAF.UI.Pages;

/// <summary>
/// Page object for the SauceDemo login page at
/// <c>https://www.saucedemo.com</c>.
/// Encapsulates all interactions with the username field, password field,
/// login button, and error message container.
/// </summary>
/// <remarks>
/// UI mechanics only — no assertions, no logging, no Allure.
/// Test authors wrap calls to this class in <c>BaseTest.Step()</c>.
/// </remarks>
public sealed class LoginPage : PageObjectBase
{
    private const string UsernameSelector = "#user-name";
    private const string PasswordSelector = "#password";
    private const string LoginSelector = "#login-btn";
    private const string ErrorSelector = ".error-message-container";

    /// <summary>
    /// Initializes a new instance of <see cref="LoginPage"/> with the
    /// Playwright page that drives this page object.
    /// </summary>
    /// <param name="page">The active Playwright <see cref="IPage"/>. Must not be null.</param>
    public LoginPage(IPage page) : base(page)
    {
    }

    /// <summary>
    /// Navigates to the SauceDemo login page.
    /// </summary>
    public async Task OpenAsync()
    {
        await Page.GotoAsync("https://www.saucedemo.com");
    }

    /// <summary>
    /// Clears the username field and types <paramref name="username"/> into it.
    /// </summary>
    /// <param name="username">The username to enter.</param>
    public async Task EnterUsernameAsync(string username)
    {
        await FillAsync(UsernameSelector, username);
    }

    /// <summary>
    /// Clears the password field and types <paramref name="password"/> into it.
    /// </summary>
    /// <param name="password">The password to enter.</param>
    public async Task EnterPasswordAsync(string password)
    {
        await FillAsync(PasswordSelector, password);
    }

    /// <summary>
    /// Clicks the login button and returns the resulting
    /// <see cref="InventoryPage"/> instance.
    /// </summary>
    /// <returns>
    /// An <see cref="InventoryPage"/> backed by the same <see cref="IPage"/>.
    /// </returns>
    public async Task<InventoryPage> ClickLoginAsync()
    {
        await ClickAsync(LoginSelector);
        return new InventoryPage(Page);
    }

    /// <summary>
    /// Convenience method that enters credentials and clicks login in sequence.
    /// This is the method most test authors will use for the happy-path login flow.
    /// </summary>
    /// <param name="username">The username to enter.</param>
    /// <param name="password">The password to enter.</param>
    /// <returns>
    /// An <see cref="InventoryPage"/> backed by the same <see cref="IPage"/>.
    /// </returns>
    public async Task<InventoryPage> LoginAsync(string username, string password)
    {
        await EnterUsernameAsync(username);
        await EnterPasswordAsync(password);
        return await ClickLoginAsync();
    }

    /// <summary>
    /// Returns the visible error message text when login fails, or
    /// <see cref="string.Empty"/> when no error container is present.
    /// Never throws — absence of the error element is a valid state.
    /// </summary>
    /// <returns>
    /// The trimmed error message string, or <see cref="string.Empty"/> if the
    /// error container is not in the DOM.
    /// </returns>
    public async Task<string> GetErrorMessageAsync()
    {
        try
        {
            return await GetTextAsync(ErrorSelector);
        }
        catch
        {
            return string.Empty;
        }
    }
}
