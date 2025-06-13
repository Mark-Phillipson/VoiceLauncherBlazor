# Talon Voice Command Search in WinForms Hybrid Application

## Overview
The Talon Voice Command Search functionality has been successfully integrated into the WinForms hybrid application. This feature allows users to search through Talon voice commands without requiring import functionality.

## How to Use

### Command Line Arguments
To launch the application directly into Talon search mode, use one of these command line arguments:

1. **Simple Talon search**: `WinFormsApp.exe "Talon search"`
2. **Alternative format**: `WinFormsApp.exe "Something" "Talon search"`

### In-App Navigation
When the application is running, you can switch to Talon search mode by:

1. **Keyboard shortcut**: Press `Alt + T` (accesskey="T")
2. **Button click**: Click the "Talon Search" button in the toolbar

### Features Available
- **Search functionality**: Search through Talon voice commands and scripts
- **Semantic search**: Toggle between exact text matching and semantic matching
- **Filtering options**:
  - Filter by Application
  - Filter by Mode
  - Filter by Operating System
- **File navigation**: Click on file paths to open them in VS Code
- **Responsive UI**: Uses Bootstrap for consistent styling

### Features NOT Available (by design)
- **Import functionality**: Import features have been intentionally excluded from the WinForms version
- **File upload**: No file upload capabilities in the hybrid version

## Implementation Details

### Code Changes Made

1. **Index.razor.cs**:
   - Added `showTalonSearch` boolean flag
   - Added `TalonSearchButtonCaption` property for dynamic button text
   - Updated command line argument parsing to detect "Talon search"
   - Added `ShowTalonSearch()` method for toggling the view
   - Updated existing navigation methods to handle Talon search state

2. **Index.razor**:
   - Added Talon Search button with accesskey="T"
   - Added conditional rendering for `TalonVoiceCommandSearch` component
   - Updated toolbar visibility logic to hide when in Talon search mode

3. **MainForm.cs**:
   - Registered `TalonVoiceCommandDataService` in the dependency injection container

### Component Reuse
The existing `TalonVoiceCommandSearch` component from the RazorClassLibrary is used as-is, ensuring consistency between the web and hybrid versions while automatically excluding import functionality.

## Testing

### Command Line Test
```bash
cd WinFormsApp
dotnet run --configuration Debug "Talon search"
```

### Development Testing
Uncomment the test line in `Index.razor.cs`:
```csharp
arguments = new string[] { arguments[0], "Talon search" };
```

## Accessibility
- Maintains hands-free development compatibility with Talon Voice
- Supports keyboard navigation and screen readers
- Uses semantic HTML elements and appropriate ARIA attributes
- Follows established accessibility patterns from the existing codebase
