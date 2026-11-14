using TUnit.Core.Interfaces;

namespace ManiaAPI.Testing;

/// <summary>
/// Marks a test or class that sends requests to the real service.
/// It puts the test in the <c>Live</c> category, so live tests can be selected with <c>--treenode-filter</c>,
/// and retries it after a short wait, because the services fail now and then on their own.
/// Live tests are skipped unless <see cref="LiveSettings.EnabledKey"/> is <c>true</c>, see <see cref="LiveSettings"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Method)]
internal sealed class LiveTestAttribute : SkipAttribute, ITestDiscoveryEventReceiver
{
    public const string Category = "Live";
    public const int RetryLimit = 2;

    /// <summary>
    /// Wait before the first retry, doubled for the next, so a short outage of the service can pass in between.
    /// </summary>
    public const int RetryBackoffMilliseconds = 2000;

    /// <summary>
    /// Whether this build targets the newest framework of the test project, the only one live tests run on, so a run of
    /// every framework doesn't send each service one set of requests per framework. Set in Directory.Build.targets.
    /// </summary>
#if LIVE_TEST_FRAMEWORK
    public const bool IsLiveTestFramework = true;
#else
    public const bool IsLiveTestFramework = false;
#endif

    public LiveTestAttribute()
        : base(IsLiveTestFramework
            ? $"Live test. Set the '{LiveSettings.EnabledKey}' user secret or the {LiveSettings.ToEnvironmentVariable(LiveSettings.EnabledKey)} environment variable to true to run it."
            : "Live test. Live tests run only on the newest target framework of the test project.")
    {
    }

    public override Task<bool> ShouldSkip(TestRegisteredContext context)
    {
        return Task.FromResult(!IsLiveTestFramework || !LiveSettings.Enabled);
    }

    public ValueTask OnTestDiscovered(DiscoveredTestContext context)
    {
        context.AddCategory(Category);
        context.SetRetryLimit(RetryLimit);
        context.SetRetryBackoff(RetryBackoffMilliseconds, backoffMultiplier: 2);
        return ValueTask.CompletedTask;
    }
}
