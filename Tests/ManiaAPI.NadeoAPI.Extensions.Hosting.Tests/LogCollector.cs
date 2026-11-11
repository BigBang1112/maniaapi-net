using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace ManiaAPI.NadeoAPI.Extensions.Hosting.Tests;

/// <summary>
/// Collects every log entry written through the host's logging.
/// </summary>
internal sealed class LogCollector : ILoggerProvider
{
    private readonly ConcurrentQueue<LogEntry> entries = new();

    public IReadOnlyCollection<LogEntry> Entries => entries;

    public ILogger CreateLogger(string categoryName) => new Logger(this, categoryName);

    public void Dispose() { }

    private sealed class Logger(LogCollector collector, string category) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            collector.entries.Enqueue(new LogEntry(category, logLevel, formatter(state, exception), exception));
        }
    }
}

internal sealed record LogEntry(string Category, LogLevel Level, string Message, Exception? Exception);
