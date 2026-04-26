# Copilot Instructions for VoiceLauncherBlazor

## Talon Voice Commands Server Project Port

For local development and UI testing, the Talon Voice Commands Server project runs on port **5008** (see `launchSettings.json`).
When using Playwright or other browser automation tools, use `http://localhost:5008` as the base URL for accessing the app. Always use the integrated browser unless asked not to.

## Terminal Interactions

### **MANDATORY: Before Any Build**
1. **Always** run port check: `powershell -ExecutionPolicy Bypass -File "./stop-processes-on-port-5008.ps1"`
2. If process is running, stop it
3. **Then** proceed with `dotnet build`

Do NOT skip this step. Check first, stop if needed, build second. Every time.

-  if you are asked to use playwright tools to demonstrate the application working always make sure the application is running first and utilize the integrated browser.

## Project Overview

- **Blazor Server Application**: Main project is a default Blazor Server app VoiceAdmin.
- **Blazor Hybrid WinForms Project**: Uses WinForms for desktop integration.
- **Component Structure**: Components use `.razor` (markup/UI) and `.razor.cs` (code-behind) files.
- **Styling**: Bootstrap for responsive UI.
- **Accessibility**: Optimized for hands-free use (Talon Voice, Cursorless) Always use access keys to access elements by keyboard shortcut/voice command.

## Development Practices

- Use `.razor` and `.razor.cs` for each component.
- Main `VoiceAdmin` project: only startup logic and DI config.
- Use `.razor.css` for component styles (no inline `<style>`).
- Use Bootstrap classes for layout and controls.
- Use semantic HTML and `aria-` attributes; ensure keyboard/voice accessibility.
- Use clear, descriptive names for files, components, and methods.
- WinForms project hosts Blazor components via BlazorWebView.

## Build Configuration

- Always use Debug builds for testing/building.
- Use XUnit for unit tests (project: TestProjectxUnit).
- Do not create console apps for tests.
- Do not use Release builds.
- Build with: `dotnet build --configuration Debug`.

## Entity Framework Migrations

- Create commandline commands for  in code blocks for transferring by voice.
- Do not run commands directly.
- Always create a script; do not use "Update Database."

## Database Configuration

- SQLite for everything (designed for personal use)
- Entity Framework Core with SQLite provider.

## Deployment Guidelines

- Blazor Hybrid WinForms: build in Release mode.

## Talon Lists and Captures

- In talon files, captures are `{}` and lists are `<>`.

## Testing the Application

- Use integrated browser as a first choice for testing and inspecting the app.
- Use Playwright tools to run and inspect the app.
- You may use Playwright for screenshots and inspection.

## Chat Messaging

- Messages may contain dictation errors.
- Interpret context and similar-sounding words as needed.
- When returning a response always please to keep it down to a minimum brevity and summarization is the key. 

## Cursorless / Terminal Commands

- When presenting terminal commands for the user, always wrap them in a fenced code block (triple backticks). This makes it easy to use the Cursorless extension or other voice/keyboard tools to pull the exact command text into a terminal without copy/paste.
- Prefer putting each command or logical command group in its own fenced block and include an appropriate language tag when known (for example, ```powershell or ```bash).
- Example (preferred):

```powershell
dotnet build --configuration Debug
```


## Run targets & ports

- **Development (recommended for manual testing):** run the `VoiceAdmin` project on port **5008**. This is the primary dev/test app used for UI/manual checks.

	```powershell
	dotnet watch --project VoiceAdmin run --urls http://localhost:5008
	```

- **If port 5008 is already bound:** stop processes using the helper script:

	```powershell
	powershell -ExecutionPolicy Bypass -File ./stop-processes-on-port-5008.ps1
	```
