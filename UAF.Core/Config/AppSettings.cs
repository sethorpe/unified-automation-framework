namespace UAF.Core.Config;

public class AppSettings
{
    public string BaseUrl { get; set; } = string.Empty;
    public BrowserSettings Browser { get; set; } = new();
    public ReportingSettings Reporting { get; set; } = new();
    public TestManagementSettings TestManagement { get; set; } = new();
    public LoggingSettings Logging { get; set; } = new();
}

public class LoggingSettings
{
    public string LogLevel { get; set; } = "Information";
}
