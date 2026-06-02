namespace UAF.Core.Base;

/// <summary>
/// Marks a single test method for pass-screenshot capture regardless of the
/// class-level <c>CaptureScreenshotOnPass</c> property or the
/// <c>ReportingSettings.CaptureScreenshotOnPass</c> config value.
/// </summary>
/// <remarks>
/// Resolution order (most specific wins):
/// <list type="number">
///   <item>This attribute on the test method — always captures on pass.</item>
///   <item><c>protected override bool CaptureScreenshotOnPass =&gt; true</c>
///         on the test class.</item>
///   <item><c>ReportingSettings.CaptureScreenshotOnPass</c> in config
///         (default: <c>false</c>).</item>
/// </list>
/// Use this attribute when a single test requires an audit trail but the rest
/// of the class does not — keeping report noise low while still satisfying
/// per-test evidence requirements.
/// </remarks>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class CaptureOnPassAttribute : Attribute
{
}