# Cursorless Visual Studio prototype

Issue [#71](https://github.com/Mark-Phillipson/VoiceLauncherBlazor/issues/71). An independent, in-process VSIX for **Visual Studio 2026 on Windows**. This milestone draws hats and provides a keyboard-accessible selection dialog; **Talon voice dispatch is not implemented yet**.

## Current behavior

- Hats cover **visible text in all text-document editors**, including CSS, XML/project files, Markdown, and plain text. Horizontal scrolling, wrapping, and outlining determine which characters can receive hats. Designer/binary views are not text editors.
- C# code uses **Roslyn lexical token spans**, without surrounding trivia. Other file types use language-neutral words and individual punctuation characters; this is not syntax-aware CSS/XML parsing. Letters, digits, underscores, and Unicode combining marks stay together; hyphens and dots are separate targets.
- **C# strings have individual word hats and an opening-quote hat for the whole string.** Ordinary, verbatim, raw, UTF-8, and interpolated strings are supported. Interpolation expressions retain their C# token targets. Word hats select source spans, not decoded runtime values; escape sequences are not separate words and remain available through the whole-string target. Empty strings still have a quote hat. If the opening quote is offscreen, only visible word hats appear. Whole-string quote targeting is C#-specific; other file types use the generic word/punctuation behavior.
- Character keys are **case-sensitive**: a hat on `A` is distinct from one on `a`. The selection dialog accepts the actual character. Future Talon integration must preserve the user's `Alpha -> A` / `Act -> a` mappings; this prototype does not alter the user's alphabet.
- Longer tokens receive priority when marks are scarce. Single-character tokens normally receive hats, but are omitted first when marks run out. A warning reports the omitted count; no duplicate/ambiguous mark is displayed.
- The prototype palette is gray (Default), blue, green, red, pink, yellow, cyan (UserColor1), and orange (UserColor2). Shapes are dot (Default), wing, curve, and bolt. Light/dark backgrounds use different contrast palettes. This is not yet a synchronized copy of VS Code settings.
- Hats do not edit the buffer or intercept mouse clicks. A small amount of space above each line keeps hats clear of the code. Assignments are retained for unchanged tokens where possible; scrolling/editing can change the map.

C# comments, whitespace, preprocessor directives, disabled preprocessor text, and parser-generated missing tokens do not receive hats. C# parsing uses a full-document standalone Roslyn syntax tree, not project-specific preprocessor symbols. Outside C#, comments are ordinary text and can receive word hats. Structural Cursorless commands are out of scope for this milestone.

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

## Talon command examples (full files included)

Talon voice dispatch is not built into this milestone, but you can automate the keyboard-driven dialog with custom Talon commands. This repository now includes a complete sample set you can copy and customize:

- [visual_studio_cursorless.talon](Talon/visual_studio_cursorless.talon)
- [visual_studio_cursorless.py](Talon/visual_studio_cursorless.py)
- [cursorless_color.talon-list](Talon/cursorless_color.talon-list)
- [cursorless_shape.talon-list](Talon/cursorless_shape.talon-list)
- [letter.talon-list](Talon/letter.talon-list)
- [symbol_key.talon-list](Talon/symbol_key.talon-list)

Copy these into your Talon user folder (or merge into your existing structure), then reload Talon.

Example commands you can speak after loading:

- `toggle hats`
- `take air`
- `take blue air`
- `take wing blue air`
- `take wing air`

Actual Talon commands (from the sample file):

```talon
take <user.cursorless_take_target_key>:
    key(alt-t)
    sleep(20ms)
    key(s)
    sleep(20ms)
    key(enter)
    sleep(20ms)
    insert(cursorless_take_target_key)
    sleep(20ms)
    key(enter)
take {user.cursorless_color} <user.cursorless_take_target_key>:
    key(alt-t)
    sleep(20ms)
    key(s)
    sleep(20ms)
    key(enter)
    sleep(20ms)
    insert(cursorless_take_target_key)
    sleep(20ms)
    key(tab)
    insert(cursorless_color)
    sleep(20ms)
    key(enter)
take {user.cursorless_shape} {user.cursorless_color} <user.cursorless_take_target_key>:
    key(alt-t)
    sleep(20ms)
    key(s)
    sleep(20ms)
    key(enter)
    sleep(20ms)
    insert(cursorless_take_target_key)
    sleep(20ms)
    key(tab)
    insert(cursorless_color)
    sleep(20ms)
    key(tab)
    insert(cursorless_shape)
    sleep(20ms)
    key(enter)
take {user.cursorless_shape} <user.cursorless_take_target_key>:
    key(alt-t)
    sleep(20ms)
    key(s)
    sleep(20ms)
    key(enter)
    sleep(20ms)
    insert(cursorless_take_target_key)
    sleep(20ms)
    key(tab:2)
    sleep(20ms)
    insert(cursorless_shape)
    sleep(20ms)
    key(enter)
toggle hats:
    key(alt-t)
    sleep(20ms)
    key(h)
    sleep(20ms)
    key(enter)
```

## Focused tests

```powershell
powershell -ExecutionPolicy Bypass -File .\stop-processes-on-port-5008.ps1
dotnet test .\TestProjectxUnit\TestProjectxUnit.csproj --configuration Debug --filter FullyQualifiedName~CursorlessHatTests
```

Tests cover uppercase/lowercase separation, lexical spans/trivia, CSS/XML/plain-text fallback, string words and opening quotes (including escaped, raw, UTF-8, nested, and interpolated strings), clipped words, multiline/disjoint visibility, assignment stability, priority on exhaustion, empty/incomplete code, Unicode, and invalid/canceled requests. Native rendering and commands must also be checked in the experimental IDE, not a browser.

Manual checks: select `A` versus `a`, scroll vertically/horizontally, toggle wrapping/outlining, edit/undo, switch documents, change zoom/theme, toggle hats, and close/reopen views. Check that document contents and dirty state are unchanged by hats/selection. A 100 ms p95 refresh target is a goal, not a measured guarantee from a single status-bar timing.

### Version 0.2.0 verified on 2026-11-12

- Debug VSIX build: zero warnings/errors; all **28** focused xUnit tests passed.
- Native VS 2026 verification selected words in a CSS stylesheet and a project XML file, selected `Uppercase` independently inside a C# string, and selected the entire string using its opening quote. File contents and dirty state stayed unchanged.
- The visual hat design is unchanged. Talon dispatch remains pending.

### Initial prototype verification (2026-11-12)

- Debug VSIX build: zero warnings/errors; all 10 focused xUnit tests passed.
- VS 2026 Community 18.12, isolated `CursorlessDemo` profile: hats rendered, all three Tools commands registered, and blue `A` / blue `a` selected different tokens without changing document contents or dirty state.
- Native checks exercised toggling, zoom, wrapping, folded method bodies, viewport scrolling in a larger C# file, document close/reopen, and clean IDE shutdown. The demo launcher was also exercised end to end.
- Talon dispatch, light-theme visual inspection, multiple IDE instances, and the full p95 performance benchmark remain unverified/not implemented as applicable. The experimental IDE was closed after validation; the launcher reopens it.
