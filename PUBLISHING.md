# Publishing Releases (ZIP artifacts)

This document explains how to create cross-platform, self-contained ZIP/TAR artifacts and make them available on GitHub Releases so they can be downloaded and tested.

---

## 1) Quick overview ✅
- Local: use `./scripts/publish.ps1` (Windows PowerShell) or `./scripts/publish.sh` (bash) to create self-contained publishes for `win-x64`, `linux-x64`, `osx-x64`.
- CI: pushing a semantic tag `v*` (for example `v1.2.3`) will trigger the GitHub Action workflow which builds for the target runtimes, packages artifacts (zip/tar) and creates a GitHub Release that contains the artifacts.

---

## 2) Local verification (fast)

Windows (PowerShell):

```powershell
# from repo root
# publish for all 3 RIDs (or specify single RIDs)
powershell.exe -ExecutionPolicy Bypass -File "scripts/publish.ps1"
# artifacts are created under ./artifacts/<rid>/
ls artifacts\win-x64
```

Linux / macOS (bash):

```bash
# publish for defaults or pass a configuration argument
./scripts/publish.sh Release
# artifacts are created under ./artifacts/<rid>/
ls artifacts/linux-x64
```

What you'll see
- `artifacts/<rid>/VoiceAdmin-<rid>.zip` (or `.tar.gz` for non-windows)
- `artifacts/win-x64/WinFormsApp-win-x64.zip` (WinForms only published on Windows)

How to test the ZIP contents
- Unzip and run the executable in the archive. Example (Windows):
  - Unzip -> open folder -> run `VoiceAdmin.exe` (it is published self-contained)
- On Linux/macOS: untar and run `./VoiceAdmin` (set +x if needed: `chmod +x VoiceAdmin`)

Notes
- The scripts publish self-contained single-file builds (`PublishSingleFile=true`); files may be large.
- If you want to test only one runtime, pass that runtime to the PowerShell script: `-runtimes @("linux-x64")`.

---

## 3) Creating a GitHub Release (triggers CI build & release)

1. Tag the commit you want to publish:

```bash
git tag v1.2.3
git push origin v1.2.3
```

2. The repo has a workflow configured to run when a `v*` tag is pushed. The workflow will:
   - Build `VoiceAdmin` for each runtime (win/linux/osx)
   - Build `WinFormsApp` only on Windows runner
   - Package the per-runtime publish into `VoiceLauncherBlazor-<runtime>.zip` (Windows) or `.tar.gz` (Linux/macOS)
   - Upload artifacts and create a GitHub Release including those artifact files

3. Monitor progress in the Actions tab. When the workflow completes, visit the **Releases** page — the new release will contain downloadable artifacts.

---

## 4) Manually uploading artifacts to a Release (optional)

If you want to build locally and attach artifacts to a release manually, use the GitHub CLI `gh` (or upload via Releases UI):

```bash
# create a release and attach files
gh release create v1.2.3 artifacts/win-x64/*.zip -R <owner>/<repo>

# or upload files to an existing release
gh release upload v1.2.3 artifacts/linux-x64/VoiceAdmin-linux-x64.zip -R <owner>/<repo>
```

To download artifacts from a release with `gh`:

```bash
gh release download v1.2.3 --pattern "*.zip" -D ./downloads
```

---

## 5) Verification checklist ✅
- [ ] Verify `artifacts/<rid>/` contains the expected archives after local publish
- [ ] Push a `v*` tag and verify the workflow builds and creates a Release
- [ ] Download and smoke-test the executable(s) from the Release asset(s)

---

## 6) Troubleshooting & tips
- If the workflow fails on a specific project (e.g., WinForms), inspect the Actions job logs to identify missing project references or platform-only dependencies.
- The publish workflow intentionally skips `WinFormsApp` for non-Windows runners.
- Artifacts uploaded directly by the workflow are attached to the GitHub Release created by the workflow; they are persistent as Release assets.

---

## 7) Want manual workflow runs?
If you'd like the ability to manually run the workflow from the Actions UI (without pushing a tag), I can add `workflow_dispatch` to the YAML so you can trigger test builds from the GitHub UI. Say the word and I’ll add it.

---

If you'd like, I can also add a small CI job that runs the created archives in a container/VM for a smoke test (extract & run process health check). Would you like that added? 🚀
