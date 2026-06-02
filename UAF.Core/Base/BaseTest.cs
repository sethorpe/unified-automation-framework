using System.Reflection;
using Allure.Net.Commons;
using Microsoft.Playwright;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using Serilog;
using UAF.Core.Config;
using UAF.Core.Driver;

namespace UAF.Core.Base;

/// <summary>
/// Abstract root base class for all NUnit test fixtures in the framework.
/// Manages the per-test Playwright page lifecycle and provides the
/// <see cref="Step(string, Action)"/> / <see cref="Step{T}(string, Func{T})"/>
/// wrappers that route execution through Allure step reporting and Serilog logging
/// in one call.
/// </summary>
/// <remarks>
/// Lifecycle contract:
/// <list type="bullet">
///   <item><c>[SetUp]</c> — creates a fresh isolated <see cref="IPage"/> via
///         <see cref="DriverManager.CreatePageAsync"/>.</item>
///   <item><c>[TearDown]</c> — captures a failure screenshot when the test did
///         not pass, then disposes the page regardless of outcome.</item>
/// </list>
/// Pass-screenshot capture:
/// <list type="bullet">
///   <item>Opt in at config level via <c>ReportingSettings.CaptureScreenshotOnPass</c>.</item>
///   <item>Opt in at class level by overriding <see cref="CaptureScreenshotOnPass"/>.</item>
///   <item>Opt in at method level with <c>[CaptureOnPass]</c> — takes highest precedence.</item>
/// </list>
/// No assertions and no page object interactions belong here.
/// </remarks>
public abstract class BaseTest
{
    /// <summary>
    /// The Playwright page created for this test by <c>[SetUp]</c> and disposed
    /// by <c>[TearDown]</c>. Scoped to a single test — never shared across tests.
    /// </summary>
    protected IPage Page { get; private set; } = null!;

    /// <summary>
    /// Controls whether a screenshot is attached to the Allure report when a
    /// step passes. Evaluated once per <see cref="Step(string, Action)"/> call
    /// after the action succeeds.
    /// </summary>
    /// <remarks>
    /// Resolution order (most specific wins):
    /// <list type="number">
    ///   <item><c>[CaptureOnPass]</c> on the test method — always captures.</item>
    ///   <item>This property overridden to <c>true</c> on the test class.</item>
    ///   <item>This base implementation — reads
    ///         <see cref="ReportingSettings.CaptureScreenshotOnPass"/> from config
    ///         (default: <c>false</c>).</item>
    /// </list>
    /// Trade-off: enabling at config or class level produces a screenshot per
    /// passing step, which grows report size quickly on long test suites.
    /// Prefer method-level opt-in unless the entire class or run requires an
    /// audit trail.
    /// </remarks>
    protected virtual bool CaptureScreenshotOnPass =>
        ConfigManager.Instance.Settings.Reporting.CaptureScreenshotOnPass;

    /// <summary>
    /// Ensures the shared browser is running, then creates a fresh isolated
    /// <see cref="IPage"/> for the current test.
    /// Called automatically by NUnit before each test method.
    /// </summary>
    [SetUp]
    public async Task SetUp()
    {
        await DriverManager.InitializeBrowserAsync();
        Page = await DriverManager.CreatePageAsync();
    }

    /// <summary>
    /// Captures a failure screenshot when the test did not pass, then disposes
    /// the page. The page is always closed — even if evidence capture throws.
    /// Called automatically by NUnit after each test method.
    /// </summary>
    [TearDown]
    public async Task TearDown()
    {
        try
        {
            if (TestContext.CurrentContext.Result.Outcome.Status != TestStatus.Passed)
            {
                try
                {
                    await CaptureEvidence("TearDown");
                }
                catch (Exception ex)
                {
                    Log.Warning(ex, "[BaseTest] TearDown evidence capture failed — continuing cleanup");
                }
            }
        }
        finally
        {
            await DriverManager.ClosePageAsync(Page);
        }
    }

    /// <summary>
    /// Executes <paramref name="action"/> as a named Allure step, writing
    /// a structured log entry before execution.
    /// If <paramref name="action"/> throws, a best-effort screenshot is captured
    /// and the original exception is rethrown unchanged so the test reports the
    /// correct failure.
    /// When the action passes and any tier of <see cref="ShouldCaptureOnPass"/>
    /// resolves to <c>true</c>, a pass screenshot is also captured and attached.
    /// </summary>
    /// <param name="stepName">
    /// Human-readable step name shown in the Allure report and the console log.
    /// </param>
    /// <param name="action">The work to perform inside this step.</param>
    protected void Step(string stepName, Action action)
    {
        Log.Information("[Step] {StepName}", stepName);

        try
        {
            AllureApi.Step(stepName, action);
        }
        catch (Exception)
        {
            try
            {
                CaptureEvidence(stepName).GetAwaiter().GetResult();
            }
            catch (Exception captureEx)
            {
                Log.Warning(captureEx, "[BaseTest] Evidence capture failed during step '{StepName}'", stepName);
            }

            throw;
        }

        if (ShouldCaptureOnPass())
        {
            try
            {
                CaptureEvidence(stepName).GetAwaiter().GetResult();
            }
            catch (Exception captureEx)
            {
                Log.Warning(captureEx, "[BaseTest] Pass evidence capture failed during step '{StepName}'", stepName);
            }
        }
    }

    /// <summary>
    /// Executes <paramref name="action"/> as a named Allure step and returns
    /// its result. Follows the same logging, evidence-capture, and rethrow
    /// contract as <see cref="Step(string, Action)"/>.
    /// </summary>
    /// <typeparam name="T">The return type of <paramref name="action"/>.</typeparam>
    /// <param name="stepName">
    /// Human-readable step name shown in the Allure report and the console log.
    /// </param>
    /// <param name="action">The work to perform inside this step.</param>
    /// <returns>The value returned by <paramref name="action"/>.</returns>
    protected T Step<T>(string stepName, Func<T> action)
    {
        Log.Information("[Step] {StepName}", stepName);

        T result;
        try
        {
            result = AllureApi.Step(stepName, action);
        }
        catch (Exception)
        {
            try
            {
                CaptureEvidence(stepName).GetAwaiter().GetResult();
            }
            catch (Exception captureEx)
            {
                Log.Warning(captureEx, "[BaseTest] Evidence capture failed during step '{StepName}'", stepName);
            }

            throw;
        }

        if (ShouldCaptureOnPass())
        {
            try
            {
                CaptureEvidence(stepName).GetAwaiter().GetResult();
            }
            catch (Exception captureEx)
            {
                Log.Warning(captureEx, "[BaseTest] Pass evidence capture failed during step '{StepName}'", stepName);
            }
        }

        return result;
    }

    /// <summary>
    /// Evaluates the three-tier resolution order and returns <c>true</c> when
    /// a pass screenshot should be captured for the currently executing test step.
    /// Exposed as <c>internal</c> so the resolution logic can be tested in
    /// <c>UAF.Tests</c> without involving Allure or screenshot capture.
    /// </summary>
    internal bool ShouldCaptureOnPass()
        => ShouldCaptureOnPass(TestContext.CurrentContext.Test.MethodName ?? string.Empty);

    /// <summary>
    /// Core resolution logic. Accepts <paramref name="methodName"/> explicitly
    /// so that unit tests can exercise all tiers without requiring a live NUnit
    /// test context.
    /// </summary>
    /// <param name="methodName">The name of the test method to inspect.</param>
    internal bool ShouldCaptureOnPass(string methodName)
    {
        // NUnit test method names are assumed to be unique within a class.
        // Overloaded test method names would cause GetMethod to throw
        // AmbiguousMatchException — NUnit itself does not support test method
        // overloads, so this assumption holds for all well-formed test classes.
        var method = GetType().GetMethod(methodName);

        if (method?.GetCustomAttribute<CaptureOnPassAttribute>() is not null)
            return true;

        return CaptureScreenshotOnPass;
    }

    /// <summary>
    /// Takes a full-page screenshot and attaches it to the current Allure step
    /// as a PNG image. All failures are swallowed and logged as warnings —
    /// this method must never propagate an exception to the caller.
    /// </summary>
    /// <param name="stepName">
    /// Used as the attachment label in the Allure report so screenshots are
    /// easy to identify alongside the step that produced them.
    /// </param>
    private async Task CaptureEvidence(string stepName)
    {
        if (Page is null)
        {
            Log.Warning("[BaseTest] CaptureEvidence skipped — Page is null (step: '{StepName}')", stepName);
            return;
        }

        try
        {
            var screenshot = await Page.ScreenshotAsync(new PageScreenshotOptions { FullPage = true });
            AllureApi.AddAttachment($"{stepName} — screenshot", "image/png", screenshot);
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "[BaseTest] Screenshot capture failed — skipping attachment (step: '{StepName}')", stepName);
        }
    }
}