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

## Testing the Application

- Use Playwright tools to run and inspect the app.
- You may use Playwright for screenshots and inspection.

## Chat Messaging

- Messages may contain dictation errors.
- Interpret context and similar-sounding words as needed.
- When returning a response always please to keep it down to a minimum brevity and summarization is the key. 

## Agent Mode Tasks

-  Do not stop the process until you have completed everything then report back and when you give your response make sure they are brief!