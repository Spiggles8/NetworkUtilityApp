using System.Text.Json;

namespace NetworkUtilityApp.Helpers;

/// <summary>
/// Simple MAC OUI (vendor) lookup helper.
///
/// Loads a JSON export of MAC prefixes (first 3 bytes / 6 hex chars)
/// and maps them to vendor names. Call <see cref="Lookup"/> with any
/// MAC string and it returns the matching vendor or "Unknown".
/// </summary>
internal static class MacVendors
{
    // Lazy so we only pay the JSON load cost when first used
    private static readonly Lazy<Dictionary<string,string>> _vendors = new(Load, isThreadSafe: true);

    /// <summary>
    /// Look up a vendor name for the given MAC address.
    ///
    /// Accepts many formats (AA-BB-CC-DD-EE-FF, AA:BB:CC..., AABB.CCDD.EEFF, etc.).
    /// Returns "Unknown" when no match is found, or empty string when input
    /// is clearly invalid.
    /// </summary>
    public static string Lookup(string? mac)
    {
        if (string.IsNullOrWhiteSpace(mac)) return string.Empty;

        // Normalize: keep only hex characters and uppercase them
        var hex = new string([.. mac.ToUpperInvariant().Where(Uri.IsHexDigit)]);
        if (hex.Length < 6) return string.Empty; // need at least one OUI (3 bytes)

        // Build AA:BB:CC key from first 6 hex chars
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

    /// <summary>
    /// Load the vendor dictionary from the JSON export on disk.
    ///
    /// The loader is intentionally tolerant: if the file is missing or
    /// unreadable we just return an empty dictionary and callers will
    /// get "Unknown" for all lookups instead of failing.
    /// </summary>
    private static Dictionary<string,string> Load()
    {
        try
        {
            string baseDir = AppContext.BaseDirectory;

            // Probe a few likely locations so it works in dev and publish layouts
            string[] candidates =
            [
                Path.Combine(baseDir, "mac-vendors-export.json"),
                Path.Combine(baseDir, "Helpers", "mac-vendors-export.json"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Helpers", "mac-vendors-export.json"),
            ];

            string? path = candidates.FirstOrDefault(File.Exists);
            if (path is null)
            {
                // Fallback for running directly from repo root
                string rel = Path.Combine("Helpers", "mac-vendors-export.json");
                path = File.Exists(rel) ? rel : null;
            }

            if (path is null)
            {
                // No data file found -> empty map
                return new Dictionary<string,string>(StringComparer.OrdinalIgnoreCase);
            }

            using var fs = File.OpenRead(path);
            var entries = JsonSerializer.Deserialize<List<Entry>>(fs) ?? [];

            // Pre-size dictionary to avoid resize churn on large vendor lists
            var dict = new Dictionary<string,string>(entries.Count, StringComparer.OrdinalIgnoreCase);
            foreach (var e in entries)
            {
                if (string.IsNullOrWhiteSpace(e.MacPrefix) || string.IsNullOrWhiteSpace(e.VendorName))
                    continue;

                // Normalize prefix: strip non-hex, take first 6, format AA:BB:CC
                var hex = new string([.. e.MacPrefix.ToUpperInvariant().Where(Uri.IsHexDigit)]);
                if (hex.Length < 6) continue;

                var key = string.Create(8, hex, (span, src) =>
                {
                    span[0] = src[0]; span[1] = src[1];
                    span[2] = ':';
                    span[3] = src[2]; span[4] = src[3];
                    span[5] = ':';
                    span[6] = src[4]; span[7] = src[5];
                });

                // Last write wins if duplicates exist in the source
                dict[key] = e.VendorName.Trim();
            }

            return dict;
        }
        catch
        {
            // On any error fall back to an empty map so callers never throw
            return new Dictionary<string,string>(StringComparer.OrdinalIgnoreCase);
        }
    }

    // Shape of a single entry in mac-vendors-export.json.
    private sealed class Entry
    {
        public string MacPrefix { get; set; } = string.Empty;
        public string VendorName { get; set; } = string.Empty;
    }
}
