using FluentAssertions;
using UAF.Core.Base;

namespace UAF.Tests.Core;

/// <summary>
/// Unit tests for <see cref="BaseTest"/>.
/// A minimal concrete subclass (<see cref="TestableTest"/>) is defined inline
/// to satisfy the abstract constraint and to promote the protected
/// <c>Step</c> overloads to a publicly callable surface.
/// These tests exercise only the Step wrapper contract — no browser is launched.
/// </summary>
[TestFixture]
public class BaseTestTests
{
    // ── Inline test double ──────────────────────────────────────────────────

    /// <summary>
    /// Minimal concrete subclass that exposes the protected <c>Step</c>
    /// overloads so the test class can invoke them directly.
    /// <c>SetUp</c> and <c>TearDown</c> are intentionally not called by
    /// these unit tests — they require a live browser (integration concern).
    /// </summary>
    private sealed class TestableTest : BaseTest
    {
        /// <summary>Calls the protected <see cref="BaseTest.Step(string, Action)"/> overload.</summary>
        public void DoStep(string name, Action action) => Step(name, action);

        /// <summary>Calls the protected <see cref="BaseTest.Step{T}(string, Func{T})"/> overload.</summary>
        public T DoStep<T>(string name, Func<T> func) => Step(name, func);
    }

    // ── Fixtures ────────────────────────────────────────────────────────────

    private TestableTest _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _sut = new TestableTest();
    }

    // ── Tests ───────────────────────────────────────────────────────────────

    [Test]
    [Category("Unit")]
    public void Step_ExecutesAction_WhenActionSucceeds()
    {
        // Arrange
        var executed = false;

        // Act
        _sut.DoStep("Execute flag step", () => executed = true);

        // Assert
        executed.Should().BeTrue();
    }

    [Test]
    [Category("Unit")]
    public void Step_RethrowsException_WhenActionThrows()
    {
        // Arrange — a step that always fails with a known message
        const string expectedMessage = "intentional step failure";
        Action failing = () => throw new InvalidOperationException(expectedMessage);

        // Act
        var act = () => _sut.DoStep("Failing step", failing);

        // Assert — original exception type and message must survive the wrapper
        act.Should().Throw<InvalidOperationException>()
           .WithMessage(expectedMessage);
    }

    [Test]
    [Category("Unit")]
    public void Step_ReturnsValue_WhenTypedOverloadSucceeds()
    {
        // Arrange
        const string expected = "hello from step";

        // Act
        var result = _sut.DoStep("Return value step", () => expected);

        // Assert
        result.Should().Be(expected);
    }

    [Test]
    [Category("Unit")]
    public void Step_RethrowsException_WhenTypedOverloadThrows()
    {
        // Arrange
        const string expectedMessage = "typed step failure";
        Func<string> failing = () => throw new ArgumentException(expectedMessage);

        // Act
        var act = () => _sut.DoStep("Failing typed step", failing);

        // Assert
        act.Should().Throw<ArgumentException>()
           .WithMessage(expectedMessage);
    }
}
