namespace NetworkUtilityApp.Models
{
    /// <summary>
    /// Backing row model for discovery results grid.
    /// </summary>
    public sealed class DiscoveryProbeRow
    {
        public string Ip { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public long? LatencyMs { get; set; }
        public string Hostname { get; set; } = string.Empty;
        public string Mac { get; set; } = string.Empty;
        public string Manufacturer { get; set; } = string.Empty;
        public string Status { get; set; } = "No Reply";
    }
}
