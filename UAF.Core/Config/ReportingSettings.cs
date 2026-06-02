namespace UAF.Core.Config;

/// <summary>
/// Reporting configuration bound from the <c>Reporting</c> section of
/// <c>appsettings.json</c>.
/// </summary>
public class ReportingSettings
{
    /// <summary>Gets or sets the list of active reporter names (e.g. <c>"Allure"</c>, <c>"Console"</c>).</summary>
    public List<string> Reporters { get; set; } = [];

    /// <summary>
    /// When <c>true</c>, every passing step captures a screenshot and attaches
    /// it to the Allure report. Default is <c>false</c>.
    /// Override at class level via <c>CaptureScreenshotOnPass</c> on
    /// <c>BaseTest</c>, or at method level via <c>[CaptureOnPass]</c>.
    /// </summary>
    public bool CaptureScreenshotOnPass { get; set; } = false;
}
