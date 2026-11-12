# Cursorless Visual Studio prototype

Issue [#71](https://github.com/Mark-Phillipson/VoiceLauncherBlazor/issues/71). An independent, in-process VSIX for **Visual Studio 2026 on Windows**. This milestone draws hats and provides a keyboard-accessible selection dialog; **Talon voice dispatch is not implemented yet**.

## Current behavior

- Hats cover **visible C# editor text**, including wrapped lines, rather than only the caret line. Horizontal scrolling and outlining determine which characters can receive hats.
- A hat refers to the **whole Roslyn lexical token span**, without surrounding trivia. One multiline token gets one hat on a visible character, even when its beginning is offscreen.
- Character keys are **case-sensitive**: a hat on `A` is distinct from one on `a`. The selection dialog accepts the actual character. Future Talon integration must preserve the user's `Alpha -> A` / `Act -> a` mappings; this prototype does not alter the user's alphabet.
- Longer tokens receive priority when marks are scarce. Single-character tokens normally receive hats, but are omitted first when marks run out. A warning reports the omitted count; no duplicate/ambiguous mark is displayed.
- The prototype palette is gray (Default), blue, green, red, pink, yellow, cyan (UserColor1), and orange (UserColor2). Shapes are dot (Default), wing, curve, and bolt. Light/dark backgrounds use different contrast palettes. This is not yet a synchronized copy of VS Code settings.
- Hats do not edit the buffer or intercept mouse clicks. A small amount of space above each line keeps hats clear of the code. Assignments are retained for unchanged tokens where possible; scrolling/editing can change the map.

Comments, whitespace, preprocessor directives, disabled preprocessor text, and parser-generated missing tokens do not receive hats. Parsing uses a full-document standalone Roslyn syntax tree, not project-specific preprocessor symbols. Structural Cursorless commands and other languages are out of scope for this milestone.

## Build

Prerequisites: the stable **.NET 10 SDK** required by the repository, Visual Studio 2026 with the **Visual Studio SDK**, and .NET Framework 4.8 targeting support. A .NET 10 RC alone does not satisfy the repository SDK pin.

Run from the repository root. The port check is mandatory before each build (including test commands that build).

```powershell
powershell -ExecutionPolicy Bypass -File .\stop-processes-on-port-5008.ps1
dotnet build .\CursorlessVisualStudio\CursorlessVisualStudio.csproj --configuration Debug
```

Output: [bin/Debug/net48/CursorlessVisualStudio.vsix](bin/Debug/net48/CursorlessVisualStudio.vsix).

The host targets .NET Framework 4.8, with portable token logic in [CursorlessVisualStudio.Core](../CursorlessVisualStudio.Core). The VSIX is deliberately not added to the application's solution, so application builds do not require Windows VSIX packaging. The existing xUnit project references only the portable library.

The project follows the installed SDK-style VSIX template, using explicit VSSDK targets and pinned 17.14 API/build packages compatible with VS 2026. The 18.12 packages were tried but depend on versions unavailable from the configured NuGet feeds. The VSIX manifest still requires VS 2026; no application framework or SDK pin was changed.

## Run safely

Close any previous **CursorlessDemo** experimental instance, then run:

```powershell
powershell -ExecutionPolicy Bypass -File .\CursorlessVisualStudio\Start-Demo.ps1
```

[Start-Demo.ps1](Start-Demo.ps1) installs the existing Debug artifact into a separate `CursorlessDemo` profile, refreshes its registration, and opens [Demo.cs](Demo.cs). It does not build, reset existing profiles, or install into the normal IDE profile. On first launch, Visual Studio may ask for sign-in or initial setup; complete that yourself or skip if offered.

Tools commands (also available for assignment in **Tools > Options > Environment > Keyboard**):

- **Cursorless Hats** (`Tools.CursorlessHats`): toggle adornments.
- **Cursorless Select Hat...** (`Tools.CursorlessSelectHat`): enter the exact character, color, and shape; press **Select**. Access keys are provided for every field and button. Default means gray/dot; UserColor1/2 are cyan/orange.
- **Cursorless Hat Diagnostics...** (`Tools.CursorlessHatDiagnostics`): show the current visible-hat count, refresh time, and capacity warning. Warnings/errors also go to Visual Studio's activity log.

The selection dialog captures the current map revision. If the document or viewport changes while it is open, selection is refused rather than targeting stale text. Close and reopen the dialog to use the new hats.

## Focused tests

```powershell
powershell -ExecutionPolicy Bypass -File .\stop-processes-on-port-5008.ps1
dotnet test .\TestProjectxUnit\TestProjectxUnit.csproj --configuration Debug --filter FullyQualifiedName~CursorlessHatTests
```

Tests cover uppercase/lowercase separation, lexical spans/trivia, multiline and disjoint visibility, assignment stability, priority on exhaustion, empty/incomplete code, Unicode, and invalid/canceled requests. Native rendering and commands must also be checked in the experimental IDE, not a browser.

Manual checks: select `A` versus `a`, scroll vertically/horizontally, toggle wrapping/outlining, edit/undo, switch documents, change zoom/theme, toggle hats, and close/reopen views. Check that document contents and dirty state are unchanged by hats/selection. A 100 ms p95 refresh target is a goal, not a measured guarantee from a single status-bar timing.

### Verified on 2026-11-12

- Debug VSIX build: zero warnings/errors; all 10 focused xUnit tests passed.
- VS 2026 Community 18.12, isolated `CursorlessDemo` profile: hats rendered, all three Tools commands registered, and blue `A` / blue `a` selected different tokens without changing document contents or dirty state.
- Native checks exercised toggling, zoom, wrapping, folded method bodies, viewport scrolling in a larger C# file, document close/reopen, and clean IDE shutdown. The demo launcher was also exercised end to end.
- Talon dispatch, light-theme visual inspection, multiple IDE instances, and the full p95 performance benchmark remain unverified/not implemented as applicable. The experimental IDE was closed after validation; the launcher reopens it.
