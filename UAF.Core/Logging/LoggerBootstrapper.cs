using Serilog;
using Serilog.Events;
using System.Diagnostics;

namespace UAF.Core.Logging;

/// <summary>
/// Owns the one-time bootstrap of the Serilog global static logger
/// (<see cref="Log.Logger"/>).
/// </summary>
/// <remarks>
/// This class exists so that logger configuration is neither the responsibility
/// of <c>ConfigManager</c> (which only loads values) nor of <c>BaseTest</c>
/// (which is per-test infrastructure). Boot concerns belong in one place —
/// assembly-level setup — and this class is the implementation detail behind
/// that setup step.
/// Consumers wire it in once from <c>AssemblySetupFixture.[OneTimeSetUp]</c>
/// and then use <see cref="Log"/> directly everywhere else.
/// </remarks>
public static class LoggerBootstrapper
{
    private const string OUTPUT_TEMPLATE =
        "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}";

    /// <summary>
    /// Configures <see cref="Log.Logger"/> with a console sink and a daily
    /// rolling file sink, then makes the logger available globally via the
    /// Serilog static <see cref="Log"/> class.
    /// </summary>
    /// <param name="logLevel">
    /// A case-insensitive Serilog <see cref="LogEventLevel"/> name
    /// (e.g. <c>"Information"</c>, <c>"Debug"</c>, <c>"Warning"</c>).
    /// If the value cannot be parsed, the level falls back to
    /// <see cref="LogEventLevel.Information"/> and a diagnostic message is
    /// written to <see cref="Debug"/> output so the misconfiguration is
    /// visible during development without throwing.
    /// </param>
    /// <param name="logDirectory">
    /// Absolute path to the directory where rolling log files are written.
    /// Created automatically if it does not exist — safe to call when the
    /// directory is already present.
    /// </param>
    public static void Initialize(string logLevel, string logDirectory)
    {
        var level = ParseLogLevel(logLevel);

        // Idempotent — no-op if the directory already exists.
        Directory.CreateDirectory(logDirectory);

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Is(level)
            .WriteTo.Console(outputTemplate: OUTPUT_TEMPLATE)
            .WriteTo.File(
                path: Path.Combine(logDirectory, "uaf-.log"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7,
                outputTemplate: OUTPUT_TEMPLATE)
            .CreateLogger();
    }

    /// <summary>
    /// Parses <paramref name="logLevel"/> into a <see cref="LogEventLevel"/>.
    /// Falls back to <see cref="LogEventLevel.Information"/> on any parse
    /// failure and emits a debug-time diagnostic — never throws.
    /// </summary>
    private static LogEventLevel ParseLogLevel(string logLevel)
    {
        if (Enum.TryParse<LogEventLevel>(logLevel, ignoreCase: true, out var parsed))
            return parsed;

        // Write to Debug output so the misconfiguration surfaces in IDE
        // diagnostics and debug-attached runs without disrupting test output.
        Debug.WriteLine(
            $"[LoggerBootstrapper] Could not parse log level '{logLevel}' — " +
            $"falling back to {LogEventLevel.Information}.");

        return LogEventLevel.Information;
    }
}
