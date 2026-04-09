# SmartComponents Setup (Quick Start)

This file contains the minimal `dotnet user-secrets` commands and notes to enable SmartComponents with Azure OpenAI and build-time Local Embeddings.

> These commands assume you are in the `VoiceAdmin` project folder (or set `--project` accordingly).

Commands:

```bash
# Enable the SmartComponents feature flag
dotnet user-secrets set "SmartComponents:Enabled" "true"

# Azure OpenAI: set API key, deployment name, and endpoint
dotnet user-secrets set "SmartComponents:ApiKey" "<your-azure-openai-key>"
dotnet user-secrets set "SmartComponents:DeploymentName" "<deployment-name>"
dotnet user-secrets set "SmartComponents:Endpoint" "https://<your-account>.openai.azure.com/"

# (Optional) If you prefer non-Azure OpenAI, set OPENAI_API_KEY env var or
# dotnet user-secrets set "OpenAI:ApiKey" "<your-openai-key>"

# Local embeddings: enabled at build-time via MSBuild property
# (Projects updated to set <UseLocalEmbeddings>true>), no user-secret required

# Quick verification: show configured SmartComponents keys
dotnet user-secrets list
``` 

Notes:
- **Local Embeddings (`UseLocalEmbeddings`) is intentionally set to `false` in all three project files** (`RazorClassLibrary`, `VoiceAdmin`, `DataAccessLibrary`). The `SmartComponents.LocalEmbeddings` NuGet build target downloads ONNX model files from `huggingface.co` at build time; that host is not reliably reachable. Keeping the flag `false` prevents the download and avoids build failures.
- Smart Paste / Smart TextArea use the OpenAI inference backend configured above.
- If local embedding support is needed in future, set `UseLocalEmbeddings=true` only on a machine with confirmed HTTPS access to `huggingface.co`, or pre-download the model files into the NuGet package cache manually.
- In production, prefer setting secrets in the host environment or in Azure Key Vault rather than user-secrets.

Troubleshooting:
- If SmartComponents do not appear active, check app logs for messages about the OpenAI backend registration.
- `CS8602` null-reference warning in `Program.cs` is suppressed at the source by guarding `smartComponentsBuilder != null` before use.
