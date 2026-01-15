namespace NetworkUtilityApp.Models
{
    /// <summary>
    /// Backing row model for the adapters grid.
    /// </summary>
    public sealed class AdaptersRow
    {
        public string AdapterName { get; set; } = string.Empty;
        public string Dhcp { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public string Subnet { get; set; } = string.Empty;
        public string Gateway { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string HardwareDetails { get; set; } = string.Empty;
        public string MacAddress { get; set; } = string.Empty;
    }
}
