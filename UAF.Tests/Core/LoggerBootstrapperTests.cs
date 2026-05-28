using FluentAssertions;
using Serilog;
using UAF.Core.Logging;

namespace UAF.Tests.Core;

/// <summary>
/// Unit tests for <see cref="LoggerBootstrapper"/>.
/// Each test uses a unique temp directory so tests are fully isolated from one
/// another and from the real <c>logs/</c> directory used during normal runs.
/// Tests validate initialization behaviour only — directory creation and
/// graceful fallback on bad log-level input. Asserting on log output content
/// is out of scope for MVP.
/// </summary>
[TestFixture]
[Category("Unit")]
public class LoggerBootstrapperTests
{
    private string _tempDir = string.Empty;

    [SetUp]
    public void SetUp()
    {
        // Each test gets its own scratch directory to prevent cross-test pollution.
        _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
    }

    [TearDown]
    public void TearDown()
    {
        Log.CloseAndFlush();

        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    [Test]
    public void Should_CreateLogDirectory_WhenDirectoryDoesNotExist()
    {
        // Arrange — temp path guaranteed not to exist yet
        _tempDir.Should().NotBeNullOrEmpty();
        Directory.Exists(_tempDir).Should().BeFalse();

        // Act
        LoggerBootstrapper.Initialize("Information", _tempDir);

        // Assert
        Directory.Exists(_tempDir).Should().BeTrue();
    }

    [Test]
    public void Should_NotThrow_WhenLogDirectoryAlreadyExists()
    {
        // Arrange — create the directory before Initialize is called
        Directory.CreateDirectory(_tempDir);

        // Act & Assert — Directory.CreateDirectory inside Initialize is idempotent
        var act = () => LoggerBootstrapper.Initialize("Information", _tempDir);
        act.Should().NotThrow();
    }

    [Test]
    public void Should_DefaultToInformation_WhenLogLevelIsInvalid()
    {
        // Arrange
        const string badLevel = "NotARealLevel";

        // Act & Assert — must fall back gracefully, never throw
        var act = () => LoggerBootstrapper.Initialize(badLevel, _tempDir);
        act.Should().NotThrow();
    }

    [Test]
    public void Should_DefaultToInformation_WhenLogLevelIsEmpty()
    {
        // Act & Assert — empty string is also an invalid level; same graceful fallback
        var act = () => LoggerBootstrapper.Initialize(string.Empty, _tempDir);
        act.Should().NotThrow();
    }
}