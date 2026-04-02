## Summary of Changes Made

### Problem
The "talon lists show" command from `global.talon` was not appearing in the TalonVoiceCommands database because the talon file parser was expecting all `.talon` files to have a header section with a `-` delimiter, but `global.talon` starts directly with commands.

### Root Cause
The parser was waiting for `inCommandsSection = true` to be set by finding a `-` delimiter, but files like `global.talon` that start directly with commands never had this delimiter, so no commands were processed.

### Solution Implemented
Modified the `TalonVoiceCommandDataService.cs` parser in two methods:

1. **`ImportFromTalonFilesAsync`** - bulk import method
2. **`ImportTalonFileContentAsync`** - single file import method

#### Changes Made:
1. **Detection Logic**: Added logic to detect if a file has a header section by scanning for a `-` delimiter
2. **No Header Handling**: If no header section is found, immediately set `inCommandsSection = true`
3. **Default Values**: When no header section exists:
   - `application` remains `"global"` (default value)
   - `modes` remains empty (default value)
   - All commands are treated as global commands

#### Code Changes:
```csharp
// First pass: check if there's a header section (look for delimiter)
bool hasHeaderSection = lines.Any(line => 
{
    var trimmed = line.Trim();
    var delimiterCheck = new string(trimmed.Where(c => !char.IsWhiteSpace(c)).ToArray());
    return delimiterCheck == "-";
});

// If no header section, start processing commands immediately
// All commands will be treated as global since there's no application/mode specification
if (!hasHeaderSection)
{
    inCommandsSection = true;
    // application remains "global" (default value)
    // modes remains empty (default value)
}
```

### Result
✅ The "talon lists show" command now appears correctly in the database:
```
talon lists show [<user.text>] | NULL | user.show_talon_lists(user.text or "") | global
```

### Compatibility
✅ **Backward Compatible**: Files with headers and delimiters continue to work as before
✅ **Forward Compatible**: Files without headers (like `global.talon`) now work correctly
✅ **Default Behavior**: Commands without headers are correctly defaulted to `application = "global"`

This fix should significantly increase the number of talon commands that get imported from files that don't follow the header+delimiter pattern.
