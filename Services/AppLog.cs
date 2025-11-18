using System;
using System.Collections.Generic;
using System.Linq;

namespace NetworkUtilityApp.Services
{
    // Basic severity tags for log entries.
    public enum LogLevel { Info, Warning, Error }

    /// <summary>
    /// Global in-memory application log.
    /// Thread-safe append, exposes an event when a new entry is added,
    /// and supports snapshot retrieval for initial UI population.
    /// </summary>
    public static class AppLog
    {
        /// <summary>
        /// Immutable log entry. Timestamp (local), level, and message text.
        /// </summary>
        public sealed class LogEntry
        {
            public DateTime Timestamp { get; init; }          // When the entry was created
            public LogLevel Level { get; init; }              // Severity
            public string Message { get; init; } = string.Empty; // User / system message
            public override string ToString() => $"[{Timestamp:yyyy-MM-dd HH:mm:ss}] [{Level}] {Message}";
        }

        /// <summary>
        /// Raised after a new entry is added. Subscribe in UI to stream updates.
        /// Sender is null (not needed); argument is the new LogEntry.
        /// </summary>
        public static event EventHandler<LogEntry>? EntryAdded;

        // Synchronization object to guard access to the list.
        private static readonly object _sync = new();
        // Backing store for entries (in-memory only). C# 12 list literal.
        private static readonly List<LogEntry> _entries = [];

        /// <summary>
        /// Returns a point-in-time copy of all log entries.
        /// Copy prevents external mutation and race conditions.
        /// </summary>
        public static IReadOnlyList<LogEntry> Snapshot()
        {
            lock (_sync) return [.. _entries]; // C# 12 spread to create a shallow copy
        }

        // Convenience helpers for common levels.
        public static void Info(string message) => Append(LogLevel.Info, message);
        public static void Warn(string message) => Append(LogLevel.Warning, message);
        public static void Error(string message) => Append(LogLevel.Error, message);

        /// <summary>
        /// Appends a new log entry if message is not empty; fires EntryAdded.
        /// </summary>
        public static void Append(LogLevel level, string message)
        {
            if (string.IsNullOrWhiteSpace(message)) return;
            var entry = new LogEntry { Timestamp = DateTime.Now, Level = level, Message = message };
            lock (_sync) _entries.Add(entry);      // Thread-safe append
            EntryAdded?.Invoke(null, entry);       // Notify subscribers (e.g., global log UI)
        }

        // Clear all log entries (thread-safe) and emit a synthetic info entry.
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