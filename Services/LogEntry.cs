namespace NetworkUtilityApp.Services
{
    /// <summary>
    /// Immutable log entry. Timestamp (local), level, and message text.
    /// </summary>
    public sealed class LogEntry
    {
        public DateTime Timestamp { get; init; }
        public LogLevel Level { get; init; }
        public string Message { get; init; } = string.Empty;
        public override string ToString()
        {
            var lvl = Level.ToString().ToUpperInvariant();
            return $"[{Timestamp:yyyy-MM-dd HH:mm:ss}] [{lvl}] {Message}";
        }
    }
}
