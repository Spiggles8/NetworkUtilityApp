namespace NetworkUtilityApp.Helpers
{
    /// <summary>
    /// Simple model for one favorite entry (IP, Subnet, Gateway).
    /// </summary>
    public sealed class FavoriteIpEntry
    {
        public string Ip { get; set; } = "";
        public string Subnet { get; set; } = "";
        public string Gateway { get; set; } = "";
    }
}
