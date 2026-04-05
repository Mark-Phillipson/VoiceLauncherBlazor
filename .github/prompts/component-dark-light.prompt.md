---
name: component-dark-light-fix
description: |
  Make the Talon UI component `TalonVoiceCommandSearch` (or another specified component) fully support both light and dark themes.
  Use this prompt when you want an automated, repeatable agent to inspect a Blazor component, find hard-coded dark/light styles, add CSS variables, and produce small targeted code patches to ensure theme compatibility.
---

Task:
- Inspect the specified Blazor component and its scoped CSS.
- Find hard-coded color values (hex, rgb, inline styles, `btn-dark`, `table-dark`, `.bg-dark`, etc.) that force dark or light coloring.
- Replace or map those values to theme-aware CSS custom properties (e.g., `--bs-body-bg`, `--bs-body-color`, `--bs-tertiary-bg`, `--bs-border-color`) and add `[data-bs-theme="dark"]` overrides when appropriate.
- Add a small `overrides-theme.css` loaded after component CSS if you need targeted runtime overrides.
- If runtime issues prevent verification (Blazor negotiate / WebSocket failures), list steps to restart the app and gather the startup exception.

Inputs:
- `component`: (optional) path or name of the component to inspect. Default: `RazorClassLibrary/Pages/TalonVoiceCommandSearch.razor`.
- `themeVars`: (optional) map of theme CSS variable names to use. Default: Bootstrap-like variables used in the repo (`--bs-body-bg`, `--bs-body-color`, `--bs-primary`, `--bs-secondary`, `--bs-tertiary-bg`, `--bs-border-color`).
- `verifyUrl`: (optional) development URL to refresh and inspect. Default: `http://localhost:5008/talon-voice-command-search`.

Outputs:
- A short list of file edits (with repo-relative paths) required to fix theming.
- Patch diffs to apply (ready to `git apply` / `apply_patch`) for CSS and minimal Razor changes.
- A step-by-step verification plan (commands to run, browser checks) and troubleshooting steps for Blazor runtime errors.

Example invocations:
- "Run this prompt for the default component and generate patches."
- "Check `Pages/TalonVoiceCommandSearch.razor.css` and produce patches only for hard-coded hex colors."

Notes & Constraints:
- Prefer minimal, focused changes. Avoid large refactors.
- If EF/Blazor runtime errors appear while verifying, include the exact terminal exception and suggest a small code fix to allow the app to start for visual verification.
- Keep CSS changes backward-compatible with existing compiled/scoped CSS by adding compatibility variable mappings.

Checklist (agent):
- [ ] Read component markup and scoped CSS
- [ ] Search repo for hex color literals related to the component
- [ ] Propose variable names and mapping strategy
- [ ] Generate minimal patches and file paths
- [ ] Provide verification steps and commands

Examples of outputs (short):
- Files changed:
  - `RazorClassLibrary/Pages/TalonVoiceCommandSearch.razor.css` — replace `background-color: #f8f9fa` with `background-color: var(--bs-body-bg)`
  - `VoiceAdmin/wwwroot/css/site.css` — add `:root { --bs-body-bg: #fff; --bs-body-color: #212529 }` and `[data-bs-theme="dark"]` overrides
  - `VoiceAdmin/wwwroot/css/overrides-theme.css` — load after component CSS to force light theme visual fixes

Ready to run: yes
