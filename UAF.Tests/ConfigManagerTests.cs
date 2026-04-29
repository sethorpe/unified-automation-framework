using FluentAssertions;
using UAF.Core.Config;

namespace UAF.Tests;

[TestFixture]
[Category("Unit")]
public class ConfigManagerTests
{
    [Test]
    public void Instance_IsNotNull()
    {
        ConfigManager.Instance.Should().NotBeNull();
    }

    [Test]
    public void Instance_ReturnsSameInstanceOnRepeatedCalls()
    {
        var first = ConfigManager.Instance;
        var second = ConfigManager.Instance;

        first.Should().BeSameAs(second);
    }

    [Test]
    public void Settings_Browser_IsNotNull()
    {
        ConfigManager.Instance.Settings.Browser.Should().NotBeNull();
    }

    [Test]
    public void Settings_BaseUrl_IsNotNullOrEmpty()
    {
        ConfigManager.Instance.Settings.BaseUrl.Should().NotBeNullOrEmpty();
    }

    [Test]
    public void Settings_Browser_Timeout_IsGreaterThanZero()
    {
        ConfigManager.Instance.Settings.Browser.Timeout.Should().BeGreaterThan(0);
    }
}
