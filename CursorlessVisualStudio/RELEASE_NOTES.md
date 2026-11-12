# Cursorless Visual Studio 0.2.0

Publisher: MSP Systems  
Author: Mark Phillipson

## Highlights

- Adds case-sensitive hat overlays for visible text in text-document editors.
- Adds C# lexical token targeting and C# string support (word hats plus opening-quote whole-string target).
- Adds keyboard-accessible commands:
  - `Tools.CursorlessHats`
  - `Tools.CursorlessSelectHat`
  - `Tools.CursorlessHatDiagnostics`
- Improves selection safety by rejecting stale selections if viewport/document changes while the dialog is open.

## Notes

- This release is a rendering and selection milestone.
- Talon voice dispatch is not built into this extension yet.
- AI-assisted development tools were used during implementation.
- This extension was inspired by the Cursorless Visual Studio Code extension; thanks to the original Cursorless authors and maintainers.
