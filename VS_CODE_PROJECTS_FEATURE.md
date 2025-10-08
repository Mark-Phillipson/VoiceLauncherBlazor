# VS Code Projects Feature

## Overview
This feature allows you to launch Visual Studio Code projects directly from the Voice Launcher without needing workspace files for each project. Simply store the folder path and the launcher will automatically open VS Code with that folder.

## Why Use This Feature?

### Before (Manual Process)
- Open terminal or command prompt
- Navigate to project directory: `cd C:\Projects\MyApp`
- Type: `code .`
- Or create a `.code-workspace` file for each project

### After (With VS Code Projects Category)
- Say "launch my app" (or your configured voice command)
- VS Code opens instantly with the project folder

## How It Works

### Category Type
Create a category with the type "VS Code Projects" (case-insensitive). The system will automatically detect launchers in this category and handle them differently from regular application launchers.

### Launcher Setup

#### Step 1: Create or Use a Category
1. Navigate to Categories management
2. Create a new category or edit an existing one
3. **Category Name**: VS Code Projects (or any name you prefer)
4. **Category Type**: `VS Code Projects` (must be exact, but case-insensitive)

#### Step 2: Create Launchers for Your Projects
1. Navigate to Launchers
2. Create a new launcher
3. Configure as follows:
   - **Name**: A friendly name for your project (e.g., "Voice Launcher Blazor")
   - **Category**: Select your "VS Code Projects" category
   - **Command Line**: Full path to the project folder (e.g., `C:\Projects\VoiceLauncherBlazor`)
   - **Working Directory**: (optional, leave empty)
   - **Arguments**: (optional, leave empty)
   - **Icon**: (optional) Choose an icon for visual identification

### Difference from Other Launchers
- **Regular Application Launchers**: Execute the CommandLine as a direct process
- **Folder Launchers**: Open File Explorer at the specified location
- **VS Code Project Launchers**: Execute `code "<folder-path>"` to open VS Code with the specified folder

## Examples

### Example 1: Personal Projects

Create launchers for your personal projects:

| Name | Category | Command Line |
|------|----------|--------------|
| Voice Launcher | VS Code Projects | `C:\Users\Mark\source\repos\VoiceLauncherBlazor` |
| My Website | VS Code Projects | `C:\Projects\MyWebsite` |
| Python Scripts | VS Code Projects | `D:\Dev\PythonScripts` |

### Example 2: Client Projects

Organize by client or project type:

| Name | Category | Command Line |
|------|----------|--------------|
| Client A Dashboard | VS Code Projects | `C:\Work\ClientA\Dashboard` |
| Client A API | VS Code Projects | `C:\Work\ClientA\API` |
| Client B Mobile | VS Code Projects | `C:\Work\ClientB\MobileApp` |

### Example 3: Mixed with Workspaces

You can still use `.code-workspace` files alongside folder paths:

| Name | Category | Command Line |
|------|----------|--------------|
| Multi-Root Workspace | Launch Applications | `C:\Projects\MyWorkspace.code-workspace` |
| Single Folder | VS Code Projects | `C:\Projects\SingleProject` |

## Voice Commands

If you're using Talon Voice, you can create voice commands to launch your projects. The exact commands depend on your Talon configuration, but typically:

- "launch voice launcher" → Opens the Voice Launcher Blazor project
- "launch my website" → Opens your website project
- "open python scripts" → Opens your Python scripts folder

## Requirements

### System Requirements
- Visual Studio Code must be installed
- The `code` command must be available in your system PATH
- On Windows, this is typically configured automatically during VS Code installation
  - If not available, you can add it manually:
    1. Open VS Code
    2. Press `Ctrl+Shift+P`
    3. Type "Shell Command: Install 'code' command in PATH"
    4. Select the option and restart your terminal

### Testing the `code` Command
1. Open a terminal/command prompt
2. Type: `code --version`
3. If you see version information, the command is properly installed

## Troubleshooting

### Issue: VS Code Doesn't Launch
**Possible Causes:**
- The `code` command is not in your PATH
- VS Code is not installed
- The folder path is incorrect

**Solutions:**
1. Verify VS Code installation
2. Test the `code` command in a terminal: `code "C:\Projects\TestFolder"`
3. Ensure the path in CommandLine is correct and accessible

### Issue: Wrong Folder Opens
**Possible Causes:**
- CommandLine has incorrect path
- Spaces in path not handled correctly

**Solutions:**
1. Verify the exact path in File Explorer
2. The system automatically wraps the path in quotes, so don't add them yourself
3. Use backslashes on Windows: `C:\Projects\MyApp` (not forward slashes)

### Issue: Multiple VS Code Windows Open
**Behavior:** This is normal if you launch multiple projects
**Solution:** If you want to open in the same window, you can:
- Close other VS Code windows first
- Or manually configure VS Code settings to reuse windows

## Implementation Details

The feature works by:
1. Checking if the launcher's category type equals "VS Code Projects" (case-insensitive)
2. If true, executing: `code "<folder-path>"`
3. If false, using the standard launcher behavior
4. This check happens in both:
   - `LauncherTable.razor.cs` - Main launcher table
   - `LauncherTableFavourites.razor.cs` - Favourites view

### Code Changes
The implementation adds minimal logic to the existing launcher execution methods:
```csharp
// Check if this is a VS Code project launcher
var category = _categories.FirstOrDefault(c => c.Id == launcher.CategoryId);
var isVSCodeProject = category?.CategoryType?.Equals("VS Code Projects", StringComparison.OrdinalIgnoreCase) == true;

// If it's a VS Code project, launch VS Code with the folder path
if (isVSCodeProject)
{
    var psi = new ProcessStartInfo();
    psi.FileName = "code";
    psi.Arguments = $"\"{cmd}\"";
    psi.UseShellExecute = true;
    psi.WindowStyle = ProcessWindowStyle.Normal;
    // ... launch process
}
```

## Benefits

1. **No Workspace Files Required**: Skip creating `.code-workspace` files for simple projects
2. **Quick Access**: Launch any project folder with a voice command
3. **Organized**: Keep all your project launchers in one category
4. **Flexible**: Works alongside traditional launchers and workspace files
5. **Voice-Optimized**: Perfect for hands-free workflow with Talon Voice

## Future Enhancements

Potential improvements that could be added:
- Support for additional editors (VS, VS Code Insiders, Sublime, etc.)
- Option to open with specific VS Code extensions pre-loaded
- Recent projects list integration
- Workspace file generation on-the-fly

## Related Features

- **Launch Applications**: Standard application launchers
- **Folder Launchers**: Open File Explorer at specific locations
- **Multiple Launchers**: Launch multiple applications at once
- **Favorites**: Quick access to your most-used launchers
