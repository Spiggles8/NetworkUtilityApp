using System.Text.Json;

namespace NetworkUtilityApp.Helpers;

internal static class MacVendors
{
    private static readonly Lazy<Dictionary<string,string>> _vendors = new(Load, isThreadSafe: true);

    public static string Lookup(string? mac)
    {
        if (string.IsNullOrWhiteSpace(mac)) return string.Empty;
        var hex = new string(mac.ToUpperInvariant().Where(Uri.IsHexDigit).ToArray());
        if (hex.Length < 6) return string.Empty;
        // Build AA:BB:CC key from first 6 hex
        var key = string.Create(8, hex, (span, src) =>
        {
            span[0] = src[0]; span[1] = src[1];
            span[2] = ':';
            span[3] = src[2]; span[4] = src[3];
            span[5] = ':';
            span[6] = src[4]; span[7] = src[5];
        });
        return _vendors.Value.TryGetValue(key, out var name) ? name : "Unknown";
    }

    private static Dictionary<string,string> Load()
    {
        try
        {
            string baseDir = AppContext.BaseDirectory;
            string[] candidates = new[]
            {
                Path.Combine(baseDir, "mac-vendors-export.json"),
                Path.Combine(baseDir, "Helpers", "mac-vendors-export.json"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Helpers", "mac-vendors-export.json"),
            };
            string? path = candidates.FirstOrDefault(File.Exists);
            if (path is null)
            {
                string rel = Path.Combine("Helpers", "mac-vendors-export.json");
                path = File.Exists(rel) ? rel : null;
            }
            if (path is null)
            {
                return new Dictionary<string,string>(StringComparer.OrdinalIgnoreCase);
            }

            using var fs = File.OpenRead(path);
            var entries = JsonSerializer.Deserialize<List<Entry>>(fs) ?? new();
            var dict = new Dictionary<string,string>(entries.Count, StringComparer.OrdinalIgnoreCase);
            foreach (var e in entries)
            {
                if (string.IsNullOrWhiteSpace(e.macPrefix) || string.IsNullOrWhiteSpace(e.vendorName))
                    continue;
                // Normalize prefix: strip non-hex, take first 6, format AA:BB:CC
                var hex = new string(e.macPrefix.ToUpperInvariant().Where(Uri.IsHexDigit).ToArray());
                if (hex.Length < 6) continue;
                var key = string.Create(8, hex, (span, src) =>
                {
                    span[0] = src[0]; span[1] = src[1];
                    span[2] = ':';
                    span[3] = src[2]; span[4] = src[3];
                    span[5] = ':';
                    span[6] = src[4]; span[7] = src[5];
                });
                dict[key] = e.vendorName.Trim();
            }
            return dict;
        }
        catch
        {
            return new Dictionary<string,string>(StringComparer.OrdinalIgnoreCase);
        }
    }

    private sealed class Entry
    {
        public string macPrefix { get; set; } = string.Empty;
        public string vendorName { get; set; } = string.Empty;
    }
}
