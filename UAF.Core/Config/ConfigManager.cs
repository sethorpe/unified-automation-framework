using Microsoft.Extensions.Configuration;

namespace UAF.Core.Config;

public sealed class ConfigManager
{
    private static readonly Lazy<ConfigManager> _instance =
        new(() => new ConfigManager(), LazyThreadSafetyMode.ExecutionAndPublication);

    public static ConfigManager Instance => _instance.Value;

    public AppSettings Settings { get; }

    private ConfigManager()
    {
        var basePath = AppContext.BaseDirectory;

        var config = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: false)
            .Build();

        Settings = config.Get<AppSettings>()
            ?? throw new InvalidOperationException(
                $"Failed to bind appsettings.json to AppSettings. Verify the file exists at: {basePath}");
    }
}
