namespace ManiaAPI.Testing;

/// <summary>
/// Marks a test or class that sends requests to the real service.
/// Live tests are skipped unless the <c>MANIAAPI_LIVE_TESTS</c> environment variable is <c>true</c> or <c>1</c>.
/// Combine it with <c>[Category("Live")]</c> so live tests can be selected with <c>--treenode-filter</c>.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Method)]
internal sealed class LiveTestAttribute : SkipAttribute
{
    public const string EnvironmentVariable = "MANIAAPI_LIVE_TESTS";

    public static bool Enabled => Environment.GetEnvironmentVariable(EnvironmentVariable) is "1" or "true" or "True" or "TRUE";

    public LiveTestAttribute() : base($"Live test. Set {EnvironmentVariable}=true to run it.")
    {
    }

    public override Task<bool> ShouldSkip(TestRegisteredContext context)
    {
        return Task.FromResult(!Enabled);
    }
}
