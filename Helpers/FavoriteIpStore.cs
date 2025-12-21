using System.Text.Json;

namespace NetworkUtilityApp.Helpers
{
    // Simple model for one favorite entry
    public sealed class FavoriteIpEntry
    {
        public string Ip { get; set; } = "";
        public string Subnet { get; set; } = "";
        public string Gateway { get; set; } = "";
    }

    // Storage for 4 favorites (+ change notification)
    public static class FavoriteIpStore
    {
        private const int Count = 4;

        // Preferred: keep beside the executable so it travels with the app (developer-friendly)
        private static readonly string ProjectFilePath = Path.Combine(AppContext.BaseDirectory, "favorites.json");

        // Fallback: AppData when write access to app folder is restricted
        private static readonly string AppDir =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "NetworkUtilityApp");
        private static readonly string AppDataFilePath = Path.Combine(AppDir, "favorites.json");

        // Reuse serializer options (avoid CA1869 allocations)
        private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

        // Raised after any save so other tabs can refresh
        public static event EventHandler? FavoritesChanged;
        
        // Get favorite at 1-based index (1..4); returns null if empty
        public static FavoriteIpEntry? Get(int index)
        {
            var all = LoadAll();
            var i = NormalizeIndex(index);
            return all[i] is { Ip.Length: > 0 } f ? f : null;
        }

        // Save favorite at 1-based index (1..4)
        public static void Save(int index, FavoriteIpEntry entry)
        {
            var all = LoadAll();
            var i = NormalizeIndex(index);
            all[i] = entry ?? new FavoriteIpEntry();
            Persist(all);
            FavoritesChanged?.Invoke(null, EventArgs.Empty);
        }

        // Load all 4 entries (never null; array length always 4)
        public static FavoriteIpEntry[] LoadAll()
        {
            // Try project-local file first
            if (TryLoad(ProjectFilePath, out var fromProject)) return EnsureSize(fromProject!);
            // Fallback to AppData
            if (TryLoad(AppDataFilePath, out var fromAppData)) return EnsureSize(fromAppData!);

            // No file found anywhere -> seed sensible defaults and persist
            var defaults = BuildDefaultFavorites();
            Persist(defaults);
            return defaults;
        }

        // Helpers
        private static bool TryLoad(string path, out FavoriteIpEntry[]? items)
        {
            items = null;
            try
            {
                if (File.Exists(path))
                {
                    var json = File.ReadAllText(path);
                    items = JsonSerializer.Deserialize<FavoriteIpEntry[]>(json, JsonOptions) ?? [];
                    return true;
                }
            }
            catch
            {
                // ignore and let caller try other location or defaults
            }
            return false;
        }

        private static FavoriteIpEntry[] EnsureSize(FavoriteIpEntry[] items)
        {
            var arr = new FavoriteIpEntry[Count];
            for (int i = 0; i < Count; i++)
                arr[i] = i < items.Length && items[i] != null ? items[i] : new FavoriteIpEntry();
            return arr;
        }

        private static int NormalizeIndex(int index) => Math.Clamp(index - 1, 0, Count - 1);

        private static void Persist(FavoriteIpEntry[] items)
        {
            // Prefer project-local path; if it fails, write to AppData
            var json = JsonSerializer.Serialize(items, JsonOptions);
            try
            {
                File.WriteAllText(ProjectFilePath, json);
            }
            catch
            {
                Directory.CreateDirectory(AppDir);
                File.WriteAllText(AppDataFilePath, json);
            }
        }

        private static FavoriteIpEntry[] BuildDefaultFavorites()
        {
            // Common home/private networks with /24 and gateway .1
            return
            [
                new FavoriteIpEntry { Ip = "192.168.1.8", Subnet = "255.255.255.0", Gateway = "192.168.1.1" },
                new FavoriteIpEntry { Ip = "192.168.0.8", Subnet = "255.255.255.0", Gateway = "192.168.0.1" },
                new FavoriteIpEntry { Ip = "10.0.0.8", Subnet = "255.255.255.0", Gateway = "10.0.0.1" },
                new FavoriteIpEntry { Ip = "172.16.0.8", Subnet = "255.255.255.0", Gateway = "172.16.0.1" },
            ];
        }
    }
}