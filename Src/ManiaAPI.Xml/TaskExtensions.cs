namespace ManiaAPI.Xml;

internal static class TaskExtensions
{
    public static IAsyncEnumerable<KeyValuePair<TKey, Task<TValue>>> WhenEach<TKey, TValue>(this IDictionary<TKey, Task<TValue>> tasks)
    {
        var processedTasks = tasks.ToDictionary(x => x.Value, x => x.Key);
        return WhenEachRemove(processedTasks);
    }

    public static async IAsyncEnumerable<KeyValuePair<TKey, Task<TValue>>> WhenEachRemove<TKey, TValue>(this IDictionary<Task<TValue>, TKey> tasks)
    {
        while (tasks.Count > 0)
        {
            var task = await Task.WhenAny(tasks.Keys);
            var platform = tasks[task];
            tasks.Remove(task);

            yield return new KeyValuePair<TKey, Task<TValue>>(platform, task);
        }
    }
}
