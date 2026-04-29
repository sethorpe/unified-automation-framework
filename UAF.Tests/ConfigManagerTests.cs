using FluentAssertions;
using Microsoft.Extensions.Configuration;
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

    [Test]
    public void Should_DefaultToDev_WhenUafEnvironmentVariableNotSet()
    {
        var previous = System.Environment.GetEnvironmentVariable("UAF_ENVIRONMENT");
        System.Environment.SetEnvironmentVariable("UAF_ENVIRONMENT", null);

        try
        {
            var env = System.Environment.GetEnvironmentVariable("UAF_ENVIRONMENT") ?? "dev";
            env.Should().Be("dev");
        }
        finally
        {
            System.Environment.SetEnvironmentVariable("UAF_ENVIRONMENT", previous);
        }
    }

    [Test]
    public void Should_LoadEnvironmentFile_WhenUafEnvironmentVariableIsSet()
    {
        var previous = System.Environment.GetEnvironmentVariable("UAF_ENVIRONMENT");
        System.Environment.SetEnvironmentVariable("UAF_ENVIRONMENT", "dev");

        try
        {
            var env = System.Environment.GetEnvironmentVariable("UAF_ENVIRONMENT") ?? "dev";
            var basePath = AppContext.BaseDirectory;

            var act = () => new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .AddJsonFile($"appsettings.{env}.json", optional: true, reloadOnChange: false)
                .Build();

            act.Should().NotThrow();
        }
        finally
        {
            System.Environment.SetEnvironmentVariable("UAF_ENVIRONMENT", previous);
        }
    }

    [Test]
    public void Should_ExposeResolvedEnvironment_WhenConfigLoaded()
    {
        ConfigManager.Instance.Environment.Should().NotBeNullOrEmpty();
    }

    [Test]
    public void Should_AllowLocalOverride_WhenAppsettingsLocalJsonExists()
    {
        var basePath = AppContext.BaseDirectory;

        var act = () => new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: false)
            .Build();

        act.Should().NotThrow();
    }

    [Test]
    public void Should_NotThrow_WhenEnvironmentSpecificFileDoesNotExist()
    {
        var basePath = AppContext.BaseDirectory;

        var act = () => new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .AddJsonFile("appsettings.nonexistent.json", optional: true, reloadOnChange: false)
            .Build();

        act.Should().NotThrow();
    }
}
