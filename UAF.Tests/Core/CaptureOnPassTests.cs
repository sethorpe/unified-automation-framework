using FluentAssertions;
using NUnit.Framework;
using UAF.Core.Base;

namespace UAF.Tests.Core;

/// <summary>
/// Unit tests for the three-tier pass-screenshot resolution logic in
/// <see cref="BaseTest.ShouldCaptureOnPass(string)"/>.
/// Tests call the <c>internal</c> overload directly so there is no dependency
/// on Allure, screenshot capture, or a real browser.
/// </summary>
/// <remarks>
/// Resolution order under test (most specific wins):
/// <list type="number">
///   <item><c>[CaptureOnPass]</c> attribute on the test method.</item>
///   <item><c>CaptureScreenshotOnPass</c> overridden to <c>true</c> on the class.</item>
///   <item>Config value via <c>ReportingSettings.CaptureScreenshotOnPass</c>
///         (reads <c>false</c> from <c>appsettings.json</c> in this assembly).</item>
/// </list>
/// </remarks>
[TestFixture]
[Category("Unit")]
public class CaptureOnPassTests
{
    // ── Test doubles ─────────────────────────────────────────────────────────

    /// <summary>
    /// No class override, no method attributes.
    /// <see cref="BaseTest.CaptureScreenshotOnPass"/> reads config (false).
    /// </summary>
    private sealed class DefaultTest : BaseTest
    {
        public void PlainMethod() { }
    }

    /// <summary>
    /// Class overrides <see cref="BaseTest.CaptureScreenshotOnPass"/> to <c>true</c>.
    /// Simulates class-level opt-in (and is logically identical to config=true).
    /// </summary>
    private sealed class ClassOptInTest : BaseTest
    {
        protected override bool CaptureScreenshotOnPass => true;

        public void PlainMethod() { }
    }

    /// <summary>
    /// Class overrides <see cref="BaseTest.CaptureScreenshotOnPass"/> to <c>false</c>.
    /// One method carries <c>[CaptureOnPass]</c>; the other does not.
    /// </summary>
    private sealed class MethodAttributeTest : BaseTest
    {
        protected override bool CaptureScreenshotOnPass => false;

        [CaptureOnPass]
        public void MarkedMethod() { }

        public void UnmarkedMethod() { }
    }

    /// <summary>
    /// Class overrides to <c>true</c> AND one method carries <c>[CaptureOnPass]</c>.
    /// Used to confirm the attribute does not conflict when both tiers opt in.
    /// </summary>
    private sealed class BothOptInTest : BaseTest
    {
        protected override bool CaptureScreenshotOnPass => true;

        [CaptureOnPass]
        public void MarkedMethod() { }
    }

    // ── Tests ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// Verifies the default: config=false, no class override, no attribute → false.
    /// </summary>
    [Test]
    public void Should_ReturnFalse_WhenConfigFalseAndNoOverrideAndNoAttribute()
    {
        var sut = new DefaultTest();
        sut.ShouldCaptureOnPass(nameof(DefaultTest.PlainMethod)).Should().BeFalse();
    }

    /// <summary>
    /// Verifies that a class-level override to <c>true</c> produces <c>true</c>
    /// even for a method with no attribute (class property > config).
    /// </summary>
    [Test]
    public void Should_ReturnTrue_WhenClassOverrideIsTrueAndNoAttribute()
    {
        var sut = new ClassOptInTest();
        sut.ShouldCaptureOnPass(nameof(ClassOptInTest.PlainMethod)).Should().BeTrue();
    }

    /// <summary>
    /// Verifies that <c>[CaptureOnPass]</c> on the method returns <c>true</c>
    /// even when the class property is <c>false</c> (method attribute > class property).
    /// </summary>
    [Test]
    public void Should_ReturnTrue_WhenMethodHasAttributeAndClassOverrideIsFalse()
    {
        var sut = new MethodAttributeTest();
        sut.ShouldCaptureOnPass(nameof(MethodAttributeTest.MarkedMethod)).Should().BeTrue();
    }

    /// <summary>
    /// Verifies that a method without <c>[CaptureOnPass]</c> returns <c>false</c>
    /// when the class property is also <c>false</c>.
    /// </summary>
    [Test]
    public void Should_ReturnFalse_WhenNoAttributeAndClassOverrideIsFalse()
    {
        var sut = new MethodAttributeTest();
        sut.ShouldCaptureOnPass(nameof(MethodAttributeTest.UnmarkedMethod)).Should().BeFalse();
    }

    /// <summary>
    /// Verifies that when both the method attribute and the class override are
    /// active, the result is still <c>true</c> (no conflict, most specific wins).
    /// </summary>
    [Test]
    public void Should_ReturnTrue_WhenBothMethodAttributeAndClassOverrideAreActive()
    {
        var sut = new BothOptInTest();
        sut.ShouldCaptureOnPass(nameof(BothOptInTest.MarkedMethod)).Should().BeTrue();
    }

    /// <summary>
    /// Verifies that an unknown method name (no match via reflection) falls back
    /// to the class property — no exception, no attribute found → false.
    /// </summary>
    [Test]
    public void Should_ReturnFalse_WhenMethodNameDoesNotExistOnType()
    {
        var sut = new DefaultTest();
        sut.ShouldCaptureOnPass("NonExistentMethod").Should().BeFalse();
    }
}