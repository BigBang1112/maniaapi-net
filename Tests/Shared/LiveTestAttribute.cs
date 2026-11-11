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

    public LiveTestAttribute()
        : base($"Live test. Set the '{LiveSettings.EnabledKey}' user secret or the {LiveSettings.ToEnvironmentVariable(LiveSettings.EnabledKey)} environment variable to true to run it.")
    {
    }

    public override Task<bool> ShouldSkip(TestRegisteredContext context)
    {
        return Task.FromResult(!LiveSettings.Enabled);
    }

    public ValueTask OnTestDiscovered(DiscoveredTestContext context)
    {
        context.AddCategory(Category);
        context.SetRetryLimit(RetryLimit);
        context.SetRetryBackoff(RetryBackoffMilliseconds, backoffMultiplier: 2);
        return ValueTask.CompletedTask;
    }
}
