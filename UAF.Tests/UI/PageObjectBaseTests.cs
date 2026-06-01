using FluentAssertions;
using Microsoft.Playwright;
using NUnit.Framework;
using UAF.Core.Base;
using UAF.UI.Pages;

namespace UAF.Tests.UI;

/// <summary>
/// Integration tests for <see cref="PageObjectBase"/>.
/// Each test renders inline HTML via <see cref="IPage.SetContentAsync"/> so
/// there is no external network dependency.
/// </summary>
/// <remarks>
/// All tests are tagged <c>Integration</c> and are excluded from the standard
/// CI pipeline via <c>--filter "TestCategory!=Integration"</c>.
/// </remarks>
[TestFixture]
[Category("Integration")]
public class PageObjectBaseTests : BaseTest
{
    private TestablePageObject _sut = null!;

    [SetUp]
    public void CreateSut()
    {
        _sut = new TestablePageObject(Page);
    }

    /// <summary>
    /// Verifies that <see cref="PageObjectBase.SelectDropdownByValueAsync"/> selects
    /// the option whose <c>value</c> attribute matches the supplied argument.
    /// </summary>
    [Test]
    public async Task Should_SelectCorrectOption_WhenSelectDropdownByValueAsyncCalled()
    {
        await Page.SetContentAsync(
            "<select id=\"s\">" +
            "<option value=\"v1\">Label 1</option>" +
            "<option value=\"v2\">Label 2</option>" +
            "</select>");

        await _sut.SelectDropdownByValueAsync("#s", "v2");

        var selected = await Page.InputValueAsync("#s");
        selected.Should().Be("v2");
    }

    /// <summary>
    /// Verifies that <see cref="PageObjectBase.SelectDropdownByTextAsync"/> selects
    /// the option whose visible label matches the supplied argument.
    /// </summary>
    [Test]
    public async Task Should_SelectCorrectOption_WhenSelectDropdownByTextAsyncCalled()
    {
        await Page.SetContentAsync(
            "<select id=\"s\">" +
            "<option value=\"v1\">Label 1</option>" +
            "<option value=\"v2\">Label 2</option>" +
            "</select>");

        await _sut.SelectDropdownByTextAsync("#s", "Label 1");

        var selected = await Page.InputValueAsync("#s");
        selected.Should().Be("v1");
    }

    /// <summary>
    /// Verifies that <see cref="PageObjectBase.IsCheckedAsync"/> returns
    /// <c>true</c> when the target checkbox carries the <c>checked</c> attribute.
    /// </summary>
    [Test]
    public async Task Should_ReturnTrue_WhenIsCheckedAsyncCalledOnCheckedCheckbox()
    {
        await Page.SetContentAsync("<input type=\"checkbox\" id=\"cb\" checked />");

        var result = await _sut.IsCheckedAsync("#cb");

        result.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that <see cref="PageObjectBase.IsCheckedAsync"/> returns
    /// <c>false</c> when the target checkbox does not carry the <c>checked</c> attribute.
    /// </summary>
    [Test]
    public async Task Should_ReturnFalse_WhenIsCheckedAsyncCalledOnUncheckedCheckbox()
    {
        await Page.SetContentAsync("<input type=\"checkbox\" id=\"cb\" />");

        var result = await _sut.IsCheckedAsync("#cb");

        result.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that <see cref="PageObjectBase.DismissAlertAsync"/> handles a
    /// browser alert without throwing and allows execution to continue normally.
    /// </summary>
    [Test]
    public async Task Should_DismissDialog_WhenDismissAlertAsyncCalled()
    {
        await Page.SetContentAsync("<button onclick=\"alert('test')\">Go</button>");

        // Register the handler before triggering the dialog.
        var dismissTask = _sut.DismissAlertAsync();
        await Page.ClickAsync("button");
        var act = async () => await dismissTask;

        await act.Should().NotThrowAsync();
    }

    /// <summary>
    /// Verifies that <see cref="PageObjectBase.AcceptAlertAsync"/> handles a
    /// browser alert without throwing and allows execution to continue normally.
    /// </summary>
    [Test]
    public async Task Should_AcceptDialog_WhenAcceptAlertAsyncCalled()
    {
        await Page.SetContentAsync("<button onclick=\"alert('test')\">Go</button>");

        // Register the handler before triggering the dialog.
        var acceptTask = _sut.AcceptAlertAsync();
        await Page.ClickAsync("button");
        var act = async () => await acceptTask;

        await act.Should().NotThrowAsync();
    }

    /// <summary>
    /// Minimal concrete subclass that makes <see cref="PageObjectBase"/> instantiable
    /// for testing without requiring a real page object.
    /// </summary>
    private sealed class TestablePageObject : PageObjectBase
    {
        public TestablePageObject(IPage page) : base(page)
        {
        }
    }
}