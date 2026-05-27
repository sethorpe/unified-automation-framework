using Allure.Net.Commons;
using Microsoft.Playwright;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using Serilog;
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
///   <item>Serilog bootstrap (issue #15) is not yet wired — <c>Log.*</c> calls
///         are no-ops until the logger is configured by the consumer.</item>
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
    /// Creates a fresh, isolated <see cref="IPage"/> for the current test.
    /// Called automatically by NUnit before each test method.
    /// </summary>
    [SetUp]
    public async Task SetUp()
    {
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
            // Only capture evidence when something went wrong — passing tests
            // do not need a screenshot attached to the report.
            if (TestContext.CurrentContext.Result.Outcome.Status != TestStatus.Passed)
            {
                try
                {
                    await CaptureEvidence("TearDown");
                }
                catch (Exception ex)
                {
                    // Evidence capture is best-effort; the original test outcome
                    // is what matters, so we swallow this and log only a warning.
                    Log.Warning(ex, "[BaseTest] TearDown evidence capture failed — continuing cleanup");
                }
            }
        }
        finally
        {
            // Page must close regardless of what happened above.
            await DriverManager.ClosePageAsync(Page);
        }
    }

    /// <summary>
    /// Executes <paramref name="action"/> as a named Allure step, writing
    /// a structured log entry before execution.
    /// If <paramref name="action"/> throws, a best-effort screenshot is captured
    /// and the original exception is rethrown unchanged so the test reports the
    /// correct failure.
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
            // Best-effort screenshot on step failure — must never mask the
            // original exception, so evidence capture runs in its own guard.
            try
            {
                CaptureEvidence(stepName).GetAwaiter().GetResult();
            }
            catch (Exception captureEx)
            {
                Log.Warning(captureEx, "[BaseTest] Evidence capture failed during step '{StepName}'", stepName);
            }

            throw; // preserve original exception type and stack trace
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

        try
        {
            return AllureApi.Step(stepName, action);
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
