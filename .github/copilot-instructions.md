# Copilot Instructions for VoiceLauncherBlazor

## Project Overview

- **Blazor Server Application**: The main project is a default Blazor Server app.
- **Blazor Hybrid WinForms Project**: Includes a Blazor Hybrid project using WinForms for desktop integration.
- **Component Structure**: Blazor components are organized with both front (`.razor`) and code-behind (`.razor.cs`) files. There are eight main components, each following this pattern.
- **Styling**: Bootstrap is used for consistent and responsive UI styling.
- **Accessibility**: The application is developed for hands-free use and enhanced accessibility, optimized for use with Talon Voice and the Cursorless Visual Studio Code Extension.

## Development Practices

- **Component Organization**: Each Blazor component consists of a `.razor` file (markup/UI) and a `.razor.cs` file (logic/code-behind).
- **CSS Isolation**: Always use CSS isolation files (`.razor.css`) for component-specific styles instead of inline `<style>` tags. This provides better separation of concerns, scoped styling, and maintainability.
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

## General Behavior
 -When running things in the terminal do not keep creating new terminals try to use the existing one!

## Build Configuration
- **IMPORTANT**: Always use Debug builds when testing or building the application.
- **Unit Tests**: Use XUnit for unit testing. Ensure all tests pass before committing changes. Project Name: TestProjectxUnit
-  do not create console apps to run tests 
- **DO NOT use Release builds** as they can break existing applications that depend on this codebase.
- When building the solution, use: `dotnet build --configuration Debug`
- Avoid commands like `dotnet build --configuration Release` or any Release configuration builds.

## Entity Framework Migrations
- Create the commandline  commands for copying pasting
- Do not run the commands directly
- Always create a script and do not use "Update Database."

## Database Configuration
- **Database Type**: SQL Server (not SQLite)
- **Entity Framework**: Uses Entity Framework Core with SQL Server provider
- **Connection**: The application connects to a SQL Server database
- **SQL Syntax**: When writing SQL queries, use SQL Server T-SQL syntax, not SQLite syntax
- **Examples**:
  - Use `SELECT TOP 10` instead of `LIMIT 10`
  - Use `ISNULL()` instead of `IFNULL()`
  - Use SQL Server date functions like `GETDATE()` instead of SQLite equivalents