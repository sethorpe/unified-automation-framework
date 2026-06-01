using Microsoft.Playwright;
using UAF.Core.Base;

namespace UAF.UI.Pages;

/// <summary>
/// Abstract base class for all concrete Playwright page objects in the UI layer.
/// Extends <see cref="BasePage"/> with higher-level helpers for dropdowns,
/// checkboxes, and browser dialogs that are too specific for the core layer
/// but common enough to belong here rather than in individual page classes.
/// </summary>
/// <remarks>
/// Design constraints:
/// <list type="bullet">
///   <item>No Allure dependency — all reporting lives in <c>BaseTest</c>.</item>
///   <item>No screenshot or attachment methods — failure evidence is captured
///         automatically by <c>BaseTest.CaptureEvidence</c> via the
///         <c>Step()</c> wrapper.</item>
///   <item>No assertions — page objects handle UI mechanics only.</item>
/// </list>
/// </remarks>
public abstract class PageObjectBase : BasePage
{
    /// <summary>
    /// Initializes a new instance of <see cref="PageObjectBase"/> with the
    /// Playwright page that drives this page object.
    /// </summary>
    /// <param name="page">
    /// The active Playwright <see cref="IPage"/>. Must not be null.
    /// </param>
    protected PageObjectBase(IPage page) : base(page)
    {
    }

    /// <summary>
    /// Selects the option whose <c>value</c> attribute matches
    /// <paramref name="value"/> in the <c>&lt;select&gt;</c> element
    /// identified by <paramref name="selector"/>.
    /// </summary>
    /// <param name="selector">A CSS, XPath, or Playwright text selector targeting a <c>&lt;select&gt;</c> element.</param>
    /// <param name="value">The <c>value</c> attribute of the option to select.</param>
    public async Task SelectDropdownByValueAsync(string selector, string value)
    {
        await Page.SelectOptionAsync(selector, new SelectOptionValue { Value = value });
    }

    /// <summary>
    /// Selects the option whose visible label text matches
    /// <paramref name="text"/> in the <c>&lt;select&gt;</c> element
    /// identified by <paramref name="selector"/>.
    /// </summary>
    /// <param name="selector">A CSS, XPath, or Playwright text selector targeting a <c>&lt;select&gt;</c> element.</param>
    /// <param name="text">The visible label text of the option to select.</param>
    public async Task SelectDropdownByTextAsync(string selector, string text)
    {
        await Page.SelectOptionAsync(selector, new SelectOptionValue { Label = text });
    }

    /// <summary>
    /// Returns <c>true</c> if the checkbox or radio button at
    /// <paramref name="selector"/> is currently checked; <c>false</c> otherwise.
    /// </summary>
    /// <param name="selector">A CSS, XPath, or Playwright text selector targeting a checkbox or radio input.</param>
    /// <returns><c>true</c> when the element is checked; otherwise <c>false</c>.</returns>
    public async Task<bool> IsCheckedAsync(string selector)
    {
        return await Page.IsCheckedAsync(selector);
    }

    /// <summary>
    /// Registers a one-time handler that dismisses the next browser dialog
    /// (alert, confirm, or prompt), then waits for the dialog event to fire.
    /// The handler is removed immediately after the dialog is dismissed so it
    /// does not affect subsequent dialogs.
    /// </summary>
    /// <remarks>
    /// Call this method before triggering the action that opens the dialog
    /// so the handler is registered before the event fires.
    /// </remarks>
    public async Task DismissAlertAsync()
    {
        var tcs = new TaskCompletionSource();
        EventHandler<IDialog>? handler = null;

        handler = async (_, dialog) =>
        {
            Page.Dialog -= handler;
            await dialog.DismissAsync();
            tcs.TrySetResult();
        };

        Page.Dialog += handler;
        await tcs.Task;
    }

    /// <summary>
    /// Registers a one-time handler that accepts the next browser dialog
    /// (alert, confirm, or prompt), then waits for the dialog event to fire.
    /// The handler is removed immediately after the dialog is accepted so it
    /// does not affect subsequent dialogs.
    /// </summary>
    /// <remarks>
    /// Call this method before triggering the action that opens the dialog
    /// so the handler is registered before the event fires.
    /// </remarks>
    public async Task AcceptAlertAsync()
    {
        var tcs = new TaskCompletionSource();
        EventHandler<IDialog>? handler = null;

        handler = async (_, dialog) =>
        {
            Page.Dialog -= handler;
            await dialog.AcceptAsync();
            tcs.TrySetResult();
        };

        Page.Dialog += handler;
        await tcs.Task;
    }
}