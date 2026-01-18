using System.IO;
using System.Text.RegularExpressions;

namespace NetworkUtilityApp.Services
{
  // Basic severity tags for log entries.
  public enum LogLevel { Info, Warning, Error, Success }

  /// <summary>
  /// Global in-memory application log. Thread-safe append, raises EntryAdded on new items,
  /// supports snapshot retrieval and simple persistence to LocalAppData.
  /// </summary>
  public static class AppLog
  {
    public static event EventHandler<LogEntry>? EntryAdded; // raised after append/clear

    private static readonly object _sync = new();
    private static readonly List<LogEntry> _entries = [];

    private static readonly Regex LinePattern = new(
        @"^\[(?<ts>\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2})\]\s*\[(?<lvl>[A-Z]+)\]\s*(?<msg>.*)$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public static IReadOnlyList<LogEntry> Snapshot()
    { lock (_sync) return [.. _entries]; }

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
      EntryAdded?.Invoke(null, new LogEntry { Timestamp = DateTime.Now, Level = LogLevel.Info, Message = string.Empty });
    }

    private static string GetLogPath()
    {
      var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "NetworkUtilityApp");
      Directory.CreateDirectory(dir);
      return Path.Combine(dir, "output-log.txt");
    }

    public static void SaveToFile()
    {
      var path = GetLogPath();
      try
      {
        // Write current snapshot
        var lines = Snapshot().Select(e => e.ToString()).ToList();
        File.WriteAllLines(path, lines);

        // Persist confirmation so it appears first after reopening when followed by additional lines
        var confirm = new LogEntry { Timestamp = DateTime.Now, Level = LogLevel.Success, Message = $"Output log saved to {path}" };
        File.AppendAllLines(path, new[] { confirm.ToString() });

        // Also add it to the in-memory log stream
        EntryAdded?.Invoke(null, confirm);
      }
      catch (Exception ex)
      {
        Error($"Failed to save output log to {path}: {ex.Message}");
      }
    }

    public static void AppendToFileAndMemory(LogLevel level, string message)
    {
      var entry = new LogEntry { Timestamp = DateTime.Now, Level = level, Message = message };
      var path = GetLogPath();
      try { File.AppendAllLines(path, new[] { entry.ToString() }); } catch { }
      lock (_sync) _entries.Add(entry);
      EntryAdded?.Invoke(null, entry);
    }

    public static void LoadFromFile()
    {
      try
      {
        var path = GetLogPath();
        if (!File.Exists(path)) return;
        var lines = File.ReadAllLines(path);
        lock (_sync)
        {
          _entries.Clear();
          foreach (var line in lines)
          {
            var m = LinePattern.Match(line);
            if (m.Success)
            {
              DateTime ts;
              if (!DateTime.TryParse(m.Groups["ts"].Value, out ts)) ts = DateTime.Now;
              var lvlStr = m.Groups["lvl"].Value;
              LogLevel lvl = LogLevel.Info;
              if (Enum.TryParse<LogLevel>(lvlStr, true, out var parsed)) lvl = parsed;
              var msg = m.Groups["msg"].Value;
              _entries.Add(new LogEntry { Timestamp = ts, Level = lvl, Message = msg });
            }
            else
            {
              _entries.Add(new LogEntry { Timestamp = DateTime.Now, Level = LogLevel.Info, Message = line });
            }
          }
        }
      }
      catch { }
    }
  }
}