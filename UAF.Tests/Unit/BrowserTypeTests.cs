using FluentAssertions;
using UAF.Core.Driver;

namespace UAF.Tests.Unit;

[TestFixture]
[Category("Unit")]
public class BrowserTypeTests
{
    [Test]
    public void Should_ContainChromium_WhenEnumIsDefined()
    {
        Enum.IsDefined(typeof(BrowserType), BrowserType.Chromium).Should().BeTrue();
    }

    [Test]
    public void Should_ContainFirefox_WhenEnumIsDefined()
    {
        Enum.IsDefined(typeof(BrowserType), BrowserType.Firefox).Should().BeTrue();
    }

    [Test]
    public void Should_ContainWebKit_WhenEnumIsDefined()
    {
        Enum.IsDefined(typeof(BrowserType), BrowserType.WebKit).Should().BeTrue();
    }

    [Test]
    public void Should_ParseFromString_WhenValidBrowserNameProvided()
    {
        var result = Enum.Parse<BrowserType>("Chromium");
        result.Should().Be(BrowserType.Chromium);
    }

    [Test]
    public void Should_ThrowArgumentException_WhenInvalidBrowserNameProvided()
    {
        var act = () => Enum.Parse<BrowserType>("Edge");
        act.Should().Throw<ArgumentException>();
    }
}
