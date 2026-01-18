# NetworkUtilityApp

A quick, focused substitute for common Windows networking tools. The goal is to make looking up and adjusting basic network info faster and easier than digging through multiple built-in UIs and consoles.

- Platform: Windows (WPF), .NET 8
- Audience: Help desk, power users, lab setups
- Scope: IPv4‑centric diagnostics, adapter management, and lightweight discovery

## Key features

- Network Adapters
  - View adapters: name, DHCP, IPv4, subnet, gateway, status, MAC, hardware details.
  - Apply DHCP or Static IP (uses `netsh`, requires admin).
  - Quick‑fill fields from favorite IP presets (saved in app).
  - Selection sync across tabs; respects visibility filters.

- Diagnostics
  - Ping (one‑shot).
  - Traceroute with optional name resolution.
  - nslookup and pathping runners with stdout/stderr capture.

- Network Discovery
  - Autofill range from selected adapter or enter start/end IPv4.
  - Cancellable parallel scan with progress, counts, and ETA.
  - Enrichment: reverse DNS, LLMNR/mDNS/NBNS fallbacks, ARP MAC, OUI vendor.
  - Save results to .txt (tab‑delimited).

- Output Log
  - Central log panel appears on each tab.
  - Clear and Save buttons; log persists between app runs.
  - On app close: saves log and writes location + closed message.
	
	- Unified Dark Mode
  - Consistent theming across tabs via Settings.

## Requirements

- Windows with .NET 8 Desktop Runtime
- Administrator rights required for:
  - Adapter changes (DHCP/static via `netsh`)
  - Some tasks that may need elevated permissions depending on system policy

## Getting started

- Visual Studio:
  1. Open the solution.
  2. Press F5 to run.
  3. Use Publish to create a distributable build (optionally self‑contained).

- Command line:
  - `dotnet run` (Debug)
  - `dotnet publish -c Release` (Publish)

## Usage notes

- Global log captures most actions and persists across runs.
- Discovery uses ICMP and ARP parsing; MAC/vendor may be unavailable off‑segment or due to OS caching.
- Traceroute parsing is tuned for English `tracert` output and is best‑effort.

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
- IPv6 support
- Route table and firewall views
- Wake‑on‑LAN and netstat summaries

## Disclaimer

This app is intended as a quick, simplified helper for common tasks. Use with care in production networks. Some operations require elevated privileges and may affect connectivity.