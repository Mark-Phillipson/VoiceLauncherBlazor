Plan — Issue #32 — Add AI-generated image button for launchers (Detailed)

Repository: Mark-Phillipson/VoiceLauncherBlazor
Issue: #32 — Add AI-generated image button for launchers

Issue summary (from Issue #32):
Add a `Generate Image` button to the launcher create/edit UI that uses the already-entered launcher metadata to generate an AI image, save it to the web app Images folder, and associate the image with the launcher.

User story:
As a user adding or editing a launcher, I can click "Generate Image" to produce an AI-generated picture derived from the launcher's title/description/tags so I can quickly get a relevant image for the launcher.

Acceptance criteria (copied / clarified):
- A `Generate Image` button appears in the launcher create/edit UI.
- Clicking it uses current launcher fields (name, command/description, categories/tags) to build a textual prompt and invoke an image-generation API.
- The generated image is saved under `wwwroot/images/` (filename pattern `launcher-<guid>.png`) and the launcher's icon/image property is set to that filename.
- The UI shows progress/feedback (spinner/placeholder) while the image is generated.
- A thumbnail (256x256) is created and stored alongside the original (e.g., `launcher-<guid>-thumb.png`).
- Errors (API, quota, network) are surfaced to the user and do not crash the UI.
- Provider API keys are loaded from configuration or environment/user-secrets (never committed).
- Optionally allow the user to accept/use/replace the generated image before saving.

Design notes / decisions:
- UI target: add button and feedback inside `RazorClassLibrary/Pages/LauncherAddEdit.razor` next to the existing icon selection UI. Server-side helper code lives in the server app (`VoiceAdmin`) as a service.
- Where to store the image: save images directly into `wwwroot/images/` so `LauncherAddEdit`'s existing `LoadImages()` discovery (which reads `wwwroot/images`) continues to work without change.
- DB field decision: the `Launcher` model already has an `Icon` string field used by the UI. To avoid an immediate EF migration, reuse `Icon` to store the generated filename. If you prefer a distinct `ImagePath` property, add it to `DataAccessLibrary.Models.Launcher`, update the DTO, and add an EF migration—this is an optional follow-up.
- Image provider: reuse code pattern from `RazorClassLibrary/Pages/AIPictures.razor.cs` (OpenAI ImageClient example). Implement a server-side, pluggable service `IImageGenerationService` with an `OpenAIImageGenerationService` implementation.
- Thumbnail: use a stable image library (recommend `SixLabors.ImageSharp`) to create a 256x256 thumbnail to avoid cross-platform System.Drawing issues.
- Security: API key read from `builder.Configuration["OpenAI:ApiKey"]` or `OPENAI_API_KEY` env var (Program.cs already references these for SmartComponents). Document setup steps.

Files to change (concrete map):
- UI: `RazorClassLibrary/Pages/LauncherAddEdit.razor` — add `Generate Image` button next to the Icon controls and a spinner/feedback area.
- UI code-behind: `RazorClassLibrary/Pages/LauncherAddEdit.razor.cs` — add `GenerateImageAsync()` which calls server endpoint, updates `imageUlrs`, and sets `LauncherDTO.Icon` to the generated filename on success.
- Sample code to reuse: `RazorClassLibrary/Pages/AIPictures.razor.cs` — reuse `ImageClient` usage to implement provider.
- Server-side service (new): `VoiceAdmin/Services/IImageGenerationService.cs` and `VoiceAdmin/Services/OpenAIImageGenerationService.cs` — generate image bytes, save file(s) to `wwwroot/images/`, create thumbnail, return file names/paths.
- Server endpoint: add minimal API route in `VoiceAdmin/Program.cs` or a small controller (e.g., `POST /api/launchers/{id}/generate-image`) which resolves launcher data, builds prompt, calls `IImageGenerationService`, updates launcher via `ILauncherRepository` and returns result.
- DTO/model changes (optional): `DataAccessLibrary/DTO/LauncherDTO.cs` and `DataAccessLibrary/Models/Launcher.cs` to add `ImagePath` if you prefer separate field (otherwise reuse existing `Icon` field). Update AutoMapper profile if DTO changed: `DataAccessLibrary/Profiles/AutoMapperProfile.cs`.
- Tests: add unit tests for `OpenAIImageGenerationService` (mock network), and integration test for the endpoint using TestServer (`TestProjectxUnit`).
- Docs: update README with instructions to set `OpenAI:ApiKey` and image storage notes.

Implementation tasks (step-by-step):
1) Summarize & confirm scope (S — 0.5–1h)
	- Confirm decision: reuse `Icon` property vs add `ImagePath` column. (If you want no migration: reuse `Icon`.)

2) Implement server-side image generation service (M — 2–6h)
	- Create `IImageGenerationService` + `OpenAIImageGenerationService` in `VoiceAdmin/Services/`.
	- Use `OpenAI.Images.ImageClient` (see `AIPictures.razor.cs`) or SmartComponents `IInferenceBackend` if available.
	- Save returned image data or download response URI bytes to `wwwroot/images/launcher-<guid>.png`.
	- Create 256x256 thumbnail with `SixLabors.ImageSharp` and save `launcher-<guid>-thumb.png`.
	- Return JSON: { "filename": "launcher-...png", "thumbnail": "launcher-...-thumb.png" }.

3) Add minimal API endpoint (S — 1–2h)
	- Add mapping in `VoiceAdmin/Program.cs`:
		- `app.MapPost("/api/launchers/{id}/generate-image", async (int id, ILauncherDataService launchers, IImageGenerationService img) => { ... });`
	- Handler: load launcher info (title, description, categories), build prompt, call image service, update launcher (set Icon or ImagePath) via `ILauncherRepository.UpdateLauncherAsync`, return result.

4) UI: add `Generate Image` button + client handling (S — 1–3h)
	- Update `RazorClassLibrary/Pages/LauncherAddEdit.razor` to add a button near the Icon area.
	- Add method `GenerateImageAsync()` to `LauncherAddEdit.razor.cs`:
		- Disable button and show spinner
		- POST to `/api/launchers/{id}/generate-image` (or send launcher JSON when no id yet)
		- On success: call `LoadImages()` to refresh `imageUlrs`, set `LauncherDTO.Icon` to returned filename, show preview
		- Handle errors and show Toast with message

5) Tests & local verification (M — 1–4h)
	- Unit test for service (mock OpenAI client).
	- Integration test for endpoint that stubs provider and verifies file creation and DB update (use temp wwwroot folder).
	- Manual test: run app locally, open create/edit UI, click Generate and verify image appears and is selectable.

6) Docs & secrets (S — 0.5–1h)
	- Document `OpenAI:ApiKey` configuration in README and in `Copilot Instructions` if needed.
	- Add entries to release notes/CHANGELOG.

7) PR & cleanup (S — 0.5–1h)
	- Branch: `issue-32/ai-image-for-launchers`
	- Include tests, sample screenshots, migration notes if any.

Estimates (summary):
- Quick/Minimal (UI stub + server endpoint + reuse `Icon`): M (4–10h).
- Full end-to-end (new DB column, robust thumbnails, ImageSharp, integration tests): L (1–3 days).

PR checklist (tailored):
- Branch name: `issue-32/ai-image-for-launchers`.
- Add/modify files listed above and register new service in `VoiceAdmin/Program.cs`.
- Include tests that cover service behavior and endpoint.
- Update `RazorClassLibrary/Pages/LauncherAddEdit.razor` UI and verify accessibility (keyboard/voice).
- Ensure `dotnet build` and `dotnet test` succeed.
- Document how to set `OpenAI:ApiKey` in README.

Next steps (what I can do now):
- Implement the minimal, safe approach: add server `IImageGenerationService`, minimal API endpoint, and UI button that reuses the existing `Icon` field (no DB migration). I can create the code and tests in the repo now.
- Or, if you prefer, I can first produce a small design PR proposing `ImagePath` DB column vs `Icon` reuse — let me know which direction you want.

Draft updated: 2026-05-01
