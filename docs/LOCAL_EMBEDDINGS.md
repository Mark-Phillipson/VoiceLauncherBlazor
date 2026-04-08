Local Embeddings / SmartComponents — quick notes

Summary
- We temporarily disabled automatic local-embeddings model downloads to avoid build failures when the Hugging Face URL is unreachable.

What changed
- Added an MSBuild feature flag `UseLocalEmbeddings` (default: `false`) to projects that referenced the package.
- `SmartComponents.LocalEmbeddings` `PackageReference` entries are now conditional on `UseLocalEmbeddings`.
- When the flag is enabled the build defines the symbol `USE_LOCAL_EMBEDDINGS` so embedding-specific code is compiled.
- When the flag is disabled, embedding usages are wrapped with `#if USE_LOCAL_EMBEDDINGS` and a safe fallback (simple substring/text search) is used.

Files modified (high level)
- DataAccessLibrary/DataAccessLibrary.csproj
- RazorClassLibrary/RazorClassLibrary.csproj
- VoiceAdmin/VoiceAdmin.csproj
- DataAccessLibrary/Services/CustomIntellisenseService.cs
- DataAccessLibrary/Services/TalonVoiceCommandDataService.cs
- RazorClassLibrary/Pages/CategoryTable.razor.cs
- RazorClassLibrary/Pages/CssPropertyTable.razor.cs
- RazorClassLibrary/Pages/TalonVoiceCommandSearch.razor.cs
- VoiceAdmin/Program.cs

Why this helps
- The `SmartComponents.LocalEmbeddings` NuGet package runs a build target that downloads model files (from Hugging Face) during restore/build; when network or SSL issues occur the build fails.
- Gating the package and code behind an opt-in flag keeps the default developer experience working while allowing you to re-enable embeddings on machines with network access.

How to re-enable local embeddings
- Enable for a single build invocation:

```powershell
dotnet build -c Release /p:UseLocalEmbeddings=true
```

- Or set `UseLocalEmbeddings` to `true` in the specific project's `.csproj` file (not recommended for CI unless you want model download on CI agents):

```xml
<PropertyGroup>
  <UseLocalEmbeddings>true</UseLocalEmbeddings>
</PropertyGroup>
```

- Or set it globally for the repo by creating a `Directory.Build.props` with the property (useful for dev machines):

```xml
<Project>
  <PropertyGroup>
    <UseLocalEmbeddings>true</UseLocalEmbeddings>
  </PropertyGroup>
</Project>
```

Notes & troubleshooting
- Enabling the flag will cause the `SmartComponents.LocalEmbeddings` package targets to attempt downloading the ONNX model and vocab files from Hugging Face during restore/build. Ensure the machine has internet access and that outbound HTTPS to `huggingface.co` is allowed.
- If the download fails with SSL/connection errors, you can:
  - Retry on a machine with working network/SSL.
  - Pre-download the model files and place them in the expected package cache path (advanced).
- The code paths that depend on the package will only compile when `USE_LOCAL_EMBEDDINGS` is defined (set by the `UseLocalEmbeddings` property). When disabled, the app uses safe substring/text fallbacks.

License/usage
- The embedding model files are hosted on Hugging Face and may have licensing terms. Review the model license before enabling and redistributing models.

If you want, I can:
- Add a short entry to `README.md` linking to this file.
- Create a `Directory.Build.props.example` that shows how to opt-in locally.
