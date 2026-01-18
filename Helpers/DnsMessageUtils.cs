using System.Net;

namespace NetworkUtilityApp.Helpers
{
  /// <summary>
  /// Shared DNS message utilities for building queries and parsing names.
  /// </summary>
  internal static class DnsMessageUtils
  {
    // Build a minimal DNS query packet for the given name and record type.
    // Header layout (12 bytes):
    // - ID (2)           : random transaction identifier
    // - Flags (2)        : 0x0000 -> standard query, recursion disabled
    // - QDCOUNT (2)      : number of questions (set to 1)
    // - ANCOUNT/NSCOUNT/ARCOUNT (6): set to 0 (no answers/authority/additional)
    // Question layout:
    // - QNAME            : sequence of labels, each prefixed with length, terminated by 0x00
    // - QTYPE (2)        : record type (e.g., 1 for A, 28 for AAAA)
    // - QCLASS (2)       : class 0x0001 (IN)
    public static byte[] BuildDnsQuery(string qname, ushort type)
    {
      var id = (ushort)Random.Shared.Next(0, 0xFFFF); // transaction ID
      var bytes = new List<byte>(128)
            {
                (byte)(id >> 8), (byte)id,      // ID
                0x00, 0x00,                     // Flags: standard query
                0x00, 0x01,                     // QDCOUNT: 1 question
                0x00, 0x00,                     // ANCOUNT: 0
                0x00, 0x00,                     // NSCOUNT: 0
                0x00, 0x00                      // ARCOUNT: 0
            };
      // Encode QNAME as length-prefixed labels
      foreach (var label in qname.Split('.'))
      {
        var lb = System.Text.Encoding.ASCII.GetBytes(label);
        bytes.Add((byte)lb.Length);
        bytes.AddRange(lb);
      }
      bytes.Add(0x00); // terminate name
      // Append QTYPE and QCLASS (IN)
      bytes.AddRange([(byte)(type >> 8), (byte)type, 0x00, 0x01]);
      return [.. bytes];
    }

    // Skip over a DNS name (handles compression pointers) and return the offset after it.
    // Name format:
    // - sequence of labels: [len][bytes...] ... 0x00 terminator
    // - compression: two high bits set (0xC0) indicates a 14-bit pointer to another location
    // This routine advances the index without allocating strings.
    public static int SkipName(byte[] buf, int off)
    {
      int i = off;
      while (i < buf.Length)
      {
        byte len = buf[i++];
        if (len == 0) break;              // end of name
        if ((len & 0xC0) == 0xC0) { i++; break; } // compression pointer consumes 2 bytes
        i += len;                          // skip label bytes
      }
      return i;
    }

    // Read a DNS name following limited compression jumps.
    // This decodes labels to a dot-separated string. Compression pointers are followed up to maxJumps.
    // If malformed data is encountered (out-of-bounds), parsing stops and returns what was decoded so far.
    public static string ReadName(byte[] buf, int off, int maxJumps = 10)
    {
      var labels = new List<string>();
      int i = off;
      int jumps = 0; // guard against infinite loops with cyclic pointers
      while (i < buf.Length && jumps < maxJumps)
      {
        byte len = buf[i++];
        if (len == 0) break; // end of name
        if ((len & 0xC0) == 0xC0)
        {
          int ptr = ((len & 0x3F) << 8) | buf[i++]; // 14-bit pointer
          i = ptr; // jump to pointer target
          jumps++;
          continue;
        }
        if (i + len > buf.Length) break; // safety: truncated label
        labels.Add(System.Text.Encoding.ASCII.GetString(buf, i, len));
        i += len;
      }
      return string.Join('.', labels);
    }
  }
}
