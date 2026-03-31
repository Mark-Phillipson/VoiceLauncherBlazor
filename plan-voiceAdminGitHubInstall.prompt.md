# Plan: Make VoiceAdmin Installable via GitHub

Cross-platform single-file self-contained releases via GitHub Actions, using the sanitized Azure SQLite DB bundled in each artifact.

**Decisions locked:** win-x64 only · osx-arm64 (M1/M2) · linux-x64 · DB as-is · WinForms Windows-only (excluded from cross-platform)

---

## Phase 1 — Fix Build Blockers *(sequential)*

1. **`VoiceAdmin/VoiceAdmin.csproj`** — Remove hardcoded `<RuntimeIdentifier>win-x86</RuntimeIdentifier>`, `<SelfContained>true</SelfContained>`, `<PublishReadyToRun>true</PublishReadyToRun>` from the main `PropertyGroup` (they override the `-r` flag in CI)
2. **`.github/workflows/publish.yml`** — Bump `dotnet-version: 9.0.x` → `10.0.x`

---

## Phase 2 — Database & Config *(parallel with Phase 1)*

3. **`VoiceAdmin/appsettings.json`** — Change SQLite connection string from absolute `C:\Users\MPhil\...` path → `Data Source=voicelauncher-azure.db` (relative, finds DB next to exe at runtime)
4. **Confirm DB tracking** — verify `voicelauncher-azure.db` is committed (not gitignored) and `VoiceAdmin.csproj` has `<CopyToOutputDirectory>Always</CopyToOutputDirectory>` so it bundles into every publish artifact

---

## Phase 3 — Workflow Polish *(depends on Phase 1)*

5. **`.github/workflows/publish.yml`** — Update matrix to `[win-x64, linux-x64, osx-arm64]`
6. **Publish command per job** — `dotnet publish -r {rid} --self-contained true -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true`; package win-x64 as `.zip`, linux/mac as `.tar.gz`; upload each as a GitHub Release asset

---

## Phase 4 — Docs *(parallel)*

7. **`README.md`** — Replace SQL Server references with SQLite, add per-platform quick-start (download → extract → run)
8. **`INSTALL.md`** *(new)* — Detailed per-platform steps, WinForms Windows-only note, env var (`ConnectionStrings__DefaultConnection`) to override DB path

---

## Relevant Files

- `.github/workflows/publish.yml` — version + matrix + publish command
- `VoiceAdmin/VoiceAdmin.csproj` — remove RID/self-contained overrides
- `VoiceAdmin/appsettings.json` — relative DB path
- `VoiceAdmin/wwwroot/voicelauncher-azure.db` — confirm tracked + copied
- `README.md` — quick-start docs
- `INSTALL.md` — new detailed install guide

---

## Verification

1. Push tag `v0.0.1-test` → all 3 matrix jobs pass in GitHub Actions
2. Extract `win-x64.zip` → `VoiceAdmin.exe` launches, data loads from bundled DB
3. Extract `linux-x64.tar.gz` → `chmod +x VoiceAdmin && ./VoiceAdmin` starts
4. Confirm `voicelauncher-azure.db` is present beside the exe in each extracted archive
