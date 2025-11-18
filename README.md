# NetworkUtilityApp

A quick, focused substitute for common Windows networking tools. The goal is to make looking up and adjusting basic network info faster and easier than digging through multiple built-in UIs and consoles.

- Platform: Windows (WinForms), .NET 8
- Audience: Help desk, power users, lab setups
- Scope: IPv4-centric day-to-day diagnostics and adapter tweaks

## Key features

- Global Output Log
  - Central log panel for all actions, with Clear and Save options.

- Network Adapters
  - View adapters: name, DHCP, IPv4, subnet, gateway, status, MAC, hardware details.
  - Apply DHCP or Static IP (uses `netsh`, requires admin).
  - Quick-fill fields from favorite IP presets (saved in app).
  - Export adapter list to .txt (tab-delimited).
  - Context menu: copy IP/MAC/row, one‑shot ping.

- Diagnostics
  - Ping (one‑shot) and Ping continuously (toggle, 2s interval).
  - Traceroute with optional name resolution.
  - nslookup and pathping with separate targets.
  - Results sent to the global log.

- Network Discovery
  - Enter start and end IPv4 (or use Autofill from active adapter).
  - Cancellable scan with progress bar, counts (scanned/active), and ETA.
  - Shows IP, Hostname (reverse DNS), MAC (via ARP), Manufacturer (OUI), Latency, Status.
  - Save results to .txt (tab‑delimited).

## Requirements

- Windows with .NET 8 Desktop Runtime
- Administrator rights required for:
  - Adapter changes (DHCP/static via `netsh`)
  - Some tasks that may need elevated permissions depending on system policy

## Getting started

- Visual Studio:
  1. Open the solution.
  2. Press F5 to run.
  3. Use __Publish__ to create a distributable build (optionally self-contained).

- Command line:
  - `dotnet run` (Debug)
  - `dotnet publish -c Release` (Publish)

## Usage notes

- Global log captures all actions. Save logs for later review.
- Discovery uses ICMP and ARP parsing; MAC/manufacturer may be unavailable for devices outside the local segment or suppressed by OS caching.
- Traceroute parsing is tuned for English `tracert` output and is best‑effort.
- Continuous ping runs until toggled off or the tab is disposed.

## Permissions

- For adapter changes, run the app as Administrator (UAC prompt) or configure an app manifest to require elevation.
- Non‑elevated runs still support viewing, diagnostics, and discovery.

## Troubleshooting

- Adapter list empty:
  - Click Refresh; ensure network interfaces are enabled.
- No MAC/manufacturer:
  - Device may not be on the same L2 segment or ARP entry is missing.
- Traceroute shows “No hops parsed”:
  - Check raw output in the log; localized output may not match parser.

## Roadmap ideas (optional)

- CSV/JSON exports
- Parallel/bounded discovery for faster scans
- IPv6 support
- Route table and firewall views
- Wake‑on‑LAN and netstat summaries

## Disclaimer

This app is intended as a quick, simplified helper for common tasks. Use with care in production networks. Some operations require elevated privileges and may affect connectivity.