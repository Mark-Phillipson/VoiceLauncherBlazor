---
name: Show Full Cards Access Key Not Working
about: Alt+C access key does not toggle the Show Full Cards checkbox
title: "[BUG] Alt+C access key not working for Show Full Cards checkbox"
labels: ['bug', 'accessibility', 'voice-control']
assignees: ''

---

## Bug Description
The Alt+C access key does not toggle the "Show Full Cards" checkbox in the Talon Voice Command Search interface, even though the access key is properly defined in the HTML.

## Expected Behavior
- Pressing Alt+C should toggle the "Show Full Cards" checkbox
- This is critical for voice control accessibility using Talon Voice

## Actual Behavior
- Alt+C does not activate the checkbox
- The access key appears to be non-functional
- Other access keys (like Alt+L for Clear Filters) work correctly

## Steps to Reproduce
1. Navigate to the Talon Voice Command Search page
2. Press Alt+C
3. Observe that the "Show Full Cards" checkbox does not toggle

## Technical Details
- **Component**: `TalonVoiceCommandSearch.razor`
- **Element**: Show Full Cards checkbox with `accesskey="c"`
- **Location**: Search controls section
- **Related**: Access key conflict was previously resolved by changing Clear Filters to Alt+L

## Environment
- **Browser**: Microsoft Edge/Chrome
- **Project**: VoiceLauncherBlazor TalonVoiceCommandsServer
- **Framework**: Blazor Server (.NET 9)

## Current Implementation
```html
<div class="form-check">
    <input class="form-check-input" type="checkbox" 
           @bind="ShowFullCards" 
           id="showFullCardsToggle" 
           accesskey="c" />
    <label class="form-check-label" for="showFullCardsToggle">
        Show Full <u>C</u>ards
    </label>
</div>
```

## Priority
**High** - This affects accessibility and voice control functionality, which is a core requirement for this application designed for hands-free use with Talon Voice.

## Possible Investigation Areas
1. Blazor event binding conflicts with HTML access keys
2. JavaScript event handlers interfering with access key functionality
3. Browser access key implementation differences
4. Need for custom JavaScript to handle access key events

## Related Issues
- Access key conflicts were previously resolved (Clear Filters changed from Alt+C to Alt+L)
- ShowFullCards toggle functionality is working correctly via mouse/touch