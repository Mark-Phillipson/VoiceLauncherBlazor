# Copilot Instructions for VoiceLauncherBlazor

## Talon Voice Commands Server Project Port

For local development and UI testing, the Talon Voice Commands Server project runs on port **5008** (see `launchSettings.json`).
When using Playwright or other browser automation tools, use `http://localhost:5008` as the base URL for accessing the app.

## Terminal Interactions

-  If you're going to run a dotnet build always check to see if the current application is running and then shut it down first.
-  if you are asked to use playwright tools to demonstrate the application working always make sure the application is running first.

## Project Overview

- **Blazor Server Application**: Main project is a default Blazor Server app.
- **Blazor Hybrid WinForms Project**: Uses WinForms for desktop integration.
- **Component Structure**: Components use `.razor` (markup/UI) and `.razor.cs` (code-behind) files.
- **Styling**: Bootstrap for responsive UI.
- **Accessibility**: Optimized for hands-free use (Talon Voice, Cursorless).

## Development Practices

- Use `.razor` and `.razor.cs` for each component.
- Main `VoiceLauncher` project: only startup logic and DI config.
- Use `.razor.css` for component styles (no inline `<style>`).
- Use Bootstrap classes for layout and controls.
- Use semantic HTML and `aria-` attributes; ensure keyboard/voice accessibility.
- Use clear, descriptive names for files, components, and methods.
- WinForms project hosts Blazor components via BlazorWebView.

## Tools

- Talon Voice, Cursorless (VS Code Extension), Visual Studio / VS Code.

## Contribution Guidelines

- Follow component structure and naming conventions.
- Maintain or improve accessibility for all UI changes.
- Test features with Talon Voice and Cursorless.
- Use Bootstrap for styling unless accessibility requires otherwise.

## Build Configuration

- Always use Debug builds for testing/building.
- Use XUnit for unit tests (project: TestProjectxUnit).
- Do not create console apps for tests.
- Do not use Release builds.
- Build with: `dotnet build --configuration Debug`.

## Entity Framework Migrations

- Create commandline commands for copying/pasting.
- Do not run commands directly.
- Always create a script; do not use "Update Database."

## Database Configuration

- SQL Server (not SQLite).
- Entity Framework Core with SQL Server provider.
- Use SQL Server T-SQL syntax (e.g., `SELECT TOP 10`, `ISNULL()`, `GETDATE()`).

## Deployment Guidelines

- Blazor Server: deploy with `dotnet publish` from command line.
- Blazor Hybrid WinForms: build in Release mode.

## Github Repository

- Repo: https://github.com/Mark-Phillipson/VoiceLauncherBlazor

## Talon Lists and Captures

- In talon files, captures are `{}` and lists are `<>`.

## Scope Clarification

All relevant projects in the solution (including `VoiceAdmin`, `RazorClassLibrary`, `TalonVoiceCommandsServer`, and shared libraries) may now be modified as needed for migration, refactoring, accessibility, or .NET 10 upgrade tasks. Remove prior restriction limiting changes to the Talon Voice Commands Server project.

## Testing the Application

- Use Playwright tools to run and inspect the app.
- You may use Playwright for screenshots and inspection.

## Chat Messaging

- Messages may contain dictation errors.
- Interpret context and similar-sounding words as needed.
- When returning a response always please to keep it down to a minimum brevity and summarization is the key. 

## Agent Mode Tasks

-  Do not stop the process until you have completed everything then report back and when you give your response make sure they are brief!

## Dark-mode flash on navigation — diagnosis & fix

- **Symptom:** When in dark mode, clicking navigation items briefly flashes the UI to a light theme then returns to dark.
- **Root cause:** Theme attribute (`data-bs-theme`) was being applied too late (client script ran after first paint) and some code used `body` instead of `html`. CSS transitions on color/background amplify the flash. Server prerender/hydration and inconsistent storage keys also contributed.
- **Quick, safe fix applied for local testing:** Add an ultra-early inline initializer in the server layout to set `data-bs-theme` on the `<html>` element before any CSS loads, and temporarily disable color/background transitions while the initializer runs.

Files changed during this session:
- `BlazorAppTestingOnly/Pages/_Layout.cshtml` — inserted inline `<style>`+`<script>` near the top of `<head>` to:
	- read canonical localStorage keys (`appTheme` then `tvc-theme`),
	- set `document.documentElement.setAttribute('data-bs-theme', ...)` early,
	- add/remove a temporary `data-theme-init` attribute to suppress transitions until first paint.
- `BlazorAppTestingOnly/Program.cs` — commented-out two service registrations (`AddIdleCircuitHandler` and `TalonVoiceCommandDataService`) temporarily to allow running the sample app for manual theme testing. These comments are test-only and should be reverted for normal development.

How to reproduce locally (manual):
1. Start the app you want to test on port 5008 (local dev target is `VoiceAdmin`):

```powershell
dotnet watch --project VoiceAdmin run --urls http://localhost:5008
```

2. Open `http://localhost:5008` in a browser (or integrated browser) and toggle dark mode.
3. Navigate between pages and verify no flash to light theme occurs.

Recommended follow-ups:
- Consolidate theme storage key to `appTheme` across projects and update theme toggle code to write that key.
- Prefer setting the theme server-side (cookie or server-rendered attribute) for authenticated users to eliminate JS reliance during first paint.
- Re-enable/comment-back the `Program.cs` service registrations after testing and ensure required packages/usings are available.

If you'd like, I can: apply the same inline initializer to other app layouts (e.g., `wwwroot/index.html` in WinForms/Static projects), consolidate the storage key usage, and revert the temporary `Program.cs` comments when you're ready.

## Run targets & ports

- **Development (recommended for manual testing):** run the `VoiceAdmin` project on port **5008**. This is the primary dev/test app used for UI/manual checks.

	```powershell
	dotnet watch --project VoiceAdmin run --urls http://localhost:5008
	```

- **If port 5008 is already bound:** stop processes using the helper script:

	```powershell
	powershell -ExecutionPolicy Bypass -File ./stop-processes-on-port-5008.ps1
	```

- **Published/sample static apps:** some sample/published builds run on ports `5000` or `5001` (e.g., `BlazorAppTestingOnly` defaults). Use those URLs when inspecting the published output.

- **Open in browser:** after starting `VoiceAdmin`, open `http://localhost:5008` in your browser (or integrated browser) to test navigation and dark-mode behavior.

Keep these commands in the repo README or your local notes so collaborators use the same ports for testing.