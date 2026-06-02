using Microsoft.Playwright;

namespace UAF.UI.Pages;

/// <summary>
/// Page object for the SauceDemo inventory (products) page.
/// </summary>
/// <remarks>
/// This is a minimal stub created on issue #18 so that <see cref="LoginPage"/>
/// has a concrete return type. The full implementation is delivered in issue #19.
/// </remarks>
public class InventoryPage : PageObjectBase
{
    /// <summary>
    /// Initializes a new instance of <see cref="InventoryPage"/> with the
    /// Playwright page that drives this page object.
    /// </summary>
    /// <param name="page">The active Playwright <see cref="IPage"/>. Must not be null.</param>
    public InventoryPage(IPage page) : base(page)
    {
    }

    /// <summary>
    /// Returns <c>true</c> when the inventory product list is visible on the page,
    /// indicating a successful post-login load.
    /// </summary>
    public async Task<bool> IsLoadedAsync()
    {
        return await IsVisibleAsync(".inventory_list");
    }
}
