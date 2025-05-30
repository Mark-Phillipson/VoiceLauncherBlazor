# Copilot Instructions for VoiceLauncherBlazor

## Project Overview

- **Blazor Server Application**: The main project is a default Blazor Server app.
- **Blazor Hybrid WinForms Project**: Includes a Blazor Hybrid project using WinForms for desktop integration.
- **Component Structure**: Blazor components are organized with both front (`.razor`) and code-behind (`.razor.cs`) files. There are eight main components, each following this pattern.
- **Styling**: Bootstrap is used for consistent and responsive UI styling.
- **Accessibility**: The application is developed for hands-free use and enhanced accessibility, optimized for use with Talon Voice and the Cursorless Visual Studio Code Extension.

## Development Practices

- **Component Organization**: Each Blazor component consists of a `.razor` file (markup/UI) and a `.razor.cs` file (logic/code-behind).
- **Styling**: Use Bootstrap classes for layout and controls. Ensure all interactive elements are accessible via keyboard and screen readers.
- **Accessibility**:
  - Use semantic HTML elements.
  - Provide `aria-` attributes where appropriate.
  - Ensure all controls are reachable and operable via voice commands and keyboard navigation.
- **Hands-Free Development**:
  - Codebase is structured for compatibility with Talon Voice and Cursorless.
  - Use clear, descriptive names for files, components, and methods to facilitate voice navigation.
- **Hybrid Integration**: The WinForms project hosts Blazor components using the BlazorWebView control.

## Tools

- **Talon Voice**: For hands-free coding and navigation.
- **Cursorless (VS Code Extension)**: For rapid, voice-driven code editing.
- **Visual Studio / VS Code**: Main IDEs for development.

## Contribution Guidelines

- Follow the component structure and naming conventions.
- Ensure all UI changes maintain or improve accessibility.
- Test new features with Talon Voice and Cursorless workflows.
- Use Bootstrap for all styling unless a specific exception is required for accessibility.

