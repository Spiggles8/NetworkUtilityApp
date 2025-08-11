using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Text;

namespace NetworkUtilityApp.Controllers
{
    public sealed class NetworkAdapterInfo
    {
        public string AdapterName { get; set; } = string.Empty;
        public string IsDhcp { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public string Subnet { get; set; } = string.Empty;
        public string Gateway { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string HardwareDetails { get; set; } = string.Empty;
        public string MacAddress { get; set; } = string.Empty;
    }


    public class NetworkController
    {
        public static List<NetworkAdapterInfo> GetAdapters()
        {
            var adaptersList = new List<NetworkAdapterInfo>();

            try
            {
                foreach (var nic in NetworkInterface.GetAllNetworkInterfaces())
                {
                    var props = nic.GetIPProperties();

                    var ipv4 = props.UnicastAddresses
                        .FirstOrDefault(ip => ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);

                    var gateway = props.GatewayAddresses
                        .FirstOrDefault(g => g.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);

                    adaptersList.Add(new NetworkAdapterInfo
                    {
                        AdapterName = nic.Name,
                        IsDhcp = props.GetIPv4Properties()?.IsDhcpEnabled == true ? "Yes" : "No",
                        IpAddress = ipv4?.Address.ToString() ?? "",
                        Subnet = ipv4?.IPv4Mask?.ToString() ?? "",
                        Gateway = gateway?.Address.ToString() ?? "",
                        Status = nic.OperationalStatus.ToString(),
                        HardwareDetails = nic.Description,
                        MacAddress = string.Join(":", nic.GetPhysicalAddress().GetAddressBytes().Select(b => b.ToString("X2")))
                    });
                }
            }
            catch (Exception ex)
            {
                adaptersList.Add(new NetworkAdapterInfo
                {
                    AdapterName = "Error",
                    Status = $"Failed to get adapters: {ex.Message}"
                });
            }

            return adaptersList;
        }

        public string SetDhcp(string adapterName)
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "netsh",
                        Arguments = $"interface ip set address \"{adapterName}\" dhcp",
                        Verb = "runas",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if (!string.IsNullOrEmpty(error))
                    return $"[ERROR] Failed to set DHCP on {adapterName}: {error}";

                return $"[SUCCESS] DHCP enabled on {adapterName}\n{output}";
            }
            catch (Exception ex)
            {
                return $"[ERROR] Exception while setting DHCP: {ex.Message}";
            }
        }

        public string SetStatic(string adapterName, string ip, string subnet, string gateway)
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "netsh",
                        Arguments = $"interface ip set address \"{adapterName}\" static {ip} {subnet} {gateway} 1",
                        Verb = "runas",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if (!string.IsNullOrEmpty(error))
                    return $"[ERROR] Failed to set Static IP on {adapterName}: {error}";

                return $"[SUCCESS] Static IP set on {adapterName} — IP: {ip}, Subnet: {subnet}, Gateway: {gateway}\n{output}";
            }
            catch (Exception ex)
            {
                return $"[ERROR] Exception while setting Static IP: {ex.Message}";
            }
        }

        public string PingHost(string ipAddress)
        {
            try
            {
                using var ping = new Ping();
                var reply = ping.Send(ipAddress, 2000); // 2s timeout

                if (reply.Status == IPStatus.Success)
                {
                    return $"[PING SUCCESS] {ipAddress} responded in {reply.RoundtripTime}ms (TTL={reply.Options?.Ttl})";
                }
                else
                {
                    return $"[PING FAIL] {ipAddress} - {reply.Status}";
                }
            }
            catch (Exception ex)
            {
                return $"[ERROR] Ping failed: {ex.Message}";
            }
        }
    }
}
