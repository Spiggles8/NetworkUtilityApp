namespace NetworkUtilityApp.Services
{
    // Basic severity tags for log entries.
    public enum LogLevel { Info, Warning, Error, Success }

    /// <summary>
    /// Global in-memory application log.
    /// Thread-safe append, exposes an event when a new entry is added,
    /// and supports snapshot retrieval for initial UI population.
    /// </summary>
    public static class AppLog
    {
        // Immutable log entry. Timestamp (local), level, and message text.
        public sealed class LogEntry
        {
            public DateTime Timestamp { get; init; }          // When the entry was created
            public LogLevel Level { get; init; }              // Severity
            public string Message { get; init; } = string.Empty; // User / system message
            public override string ToString()
            {
                var lvl = Level.ToString().ToUpperInvariant();
                return $"[{Timestamp:yyyy-MM-dd HH:mm:ss}] [{lvl}] {Message}";
            }
        }

        /// <summary>
        /// Raised after a new entry is added. Subscribe in UI to stream updates.
        /// Sender is null (not needed); argument is the new LogEntry.
        /// </summary>
        public static event EventHandler<LogEntry>? EntryAdded;

        private static readonly object _sync = new();
        private static readonly List<LogEntry> _entries = [];

        public static IReadOnlyList<LogEntry> Snapshot()
        {
            lock (_sync) return [.. _entries];
        }

        public static void Info(string message) => Append(LogLevel.Info, message);
        public static void Warn(string message) => Append(LogLevel.Warning, message);
        public static void Error(string message) => Append(LogLevel.Error, message);
        public static void Success(string message) => Append(LogLevel.Success, message);

        public static void Append(LogLevel level, string message)
        {
            if (string.IsNullOrWhiteSpace(message)) return;
            var entry = new LogEntry { Timestamp = DateTime.Now, Level = level, Message = message };
            lock (_sync) _entries.Add(entry);
            EntryAdded?.Invoke(null, entry);
        }

        public static void Clear()
        {
            lock (_sync) _entries.Clear();
            EntryAdded?.Invoke(null, new LogEntry
            {
                Timestamp = DateTime.Now,
                Level = LogLevel.Info,
                Message = "(log cleared)"
            });
        }
    }
}