using System;

namespace NetworkUtilityApp.Views
{
    /// <summary>
    /// SettingsView partial: small validation/parsing helpers used by settings UI.
    /// </summary>
    public partial class SettingsView
    {
        private static int? ParseIntOrNull(string text, int min, int max)
        {
            if (int.TryParse(text, out var v) && v >= min && v <= max) return v;
            return null;
        }

        private static string JoinOctets(string a, string b, string c, string d)
        {
            string oa = Octet(a), ob = Octet(b), oc = Octet(c), od = Octet(d);
            if (oa.Length == 0 || ob.Length == 0 || oc.Length == 0 || od.Length == 0) return string.Empty;
            return string.Join('.', oa, ob, oc, od);
        }

        private static string Octet(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return string.Empty;
            if (int.TryParse(s, out var v) && v >= 0 && v <= 255) return v.ToString();
            return string.Empty;
        }
    }
}
