# VS Code Projects Feature

## Overview
This feature allows you to launch Visual Studio Code projects directly from the Voice Launcher without needing workspace files for each project.

## How It Works

### Category Type
Create a category with the type "VS Code Projects" (case-insensitive).

### Launcher Setup
1. Create a launcher and assign it to a category with type "VS Code Projects"
2. In the CommandLine field, enter the full path to the project folder (e.g., `C:\Projects\MyApp`)
3. The launcher will automatically execute `code "<folder-path>"` when activated

### Difference from Folder Launchers
- **Folder launchers**: Open File Explorer at the specified location
- **VS Code Project launchers**: Open Visual Studio Code with the specified folder

## Example

### Create a Category
- **Category Name**: VS Code Projects
- **Category Type**: VS Code Projects

### Create a Launcher
- **Name**: Voice Launcher Blazor
- **Category**: VS Code Projects
- **Command Line**: `C:\Users\YourName\source\repos\VoiceLauncherBlazor`
- **Working Directory**: (optional)
- **Arguments**: (leave empty)

When you activate this launcher, it will execute:
```
code "C:\Users\YourName\source\repos\VoiceLauncherBlazor"
```

This will open VS Code with that folder as the workspace.

## Requirements
- Visual Studio Code must be installed and the `code` command must be available in your system PATH
- On Windows, this is typically configured automatically during VS Code installation

## Implementation Details
The feature works by:
1. Checking if the launcher's category type equals "VS Code Projects"
2. If true, launching `code "<folder-path>"` instead of the standard process execution
3. This works in both the main launcher table and the favourites table
