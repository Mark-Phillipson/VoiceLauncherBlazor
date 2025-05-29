## GitHub Copilot Chat

- Extension Version: 0.28.2025052901 (prod)
- VS Code: vscode/1.101.0-insider
- OS: Windows

## Network

User Settings:
```json
  "github.copilot.advanced.debug.useElectronFetcher": true,
  "github.copilot.advanced.debug.useNodeFetcher": false,
  "github.copilot.advanced.debug.useNodeFetchFetcher": true
```

Connecting to https://api.github.com:
- DNS ipv4 Lookup: 20.26.156.210 (8 ms)
- DNS ipv6 Lookup: Error (9 ms): getaddrinfo ENOTFOUND api.github.com
- Proxy URL: None (1 ms)
- Electron fetch (configured): HTTP 200 (3093 ms)
- Node.js https: HTTP 200 (47 ms)
- Node.js fetch: HTTP 200 (158 ms)
- Helix fetch: HTTP 200 (62 ms)

Connecting to https://api.individual.githubcopilot.com/_ping:
- DNS ipv4 Lookup: 140.82.113.21 (9 ms)
- DNS ipv6 Lookup: Error (8 ms): getaddrinfo ENOTFOUND api.individual.githubcopilot.com
- Proxy URL: None (3 ms)
- Electron fetch (configured): HTTP 200 (84 ms)
- Node.js https: HTTP 200 (262 ms)
- Node.js fetch: HTTP 200 (265 ms)
- Helix fetch: HTTP 200 (277 ms)

## Documentation

In corporate networks: [Troubleshooting firewall settings for GitHub Copilot](https://docs.github.com/en/copilot/troubleshooting-github-copilot/troubleshooting-firewall-settings-for-github-copilot).