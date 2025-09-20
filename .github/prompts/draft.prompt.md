Implement an automatic rotating display of random voice commands under the following conditions:

Prerequisites:
- Search TextBox must be empty
- No active filters applied
- Auto Filter by app setting must be disabled
- Page must be fully loaded (DOMContentLoaded event)

Core Functionality:
1. Create a JavaScript function that:
   - Fetches the complete list of available voice commands from the data source
   - Randomly selects one command at a time
   - Updates the display area with the selected command
   - Maintains a configurable rotation interval (e.g., 7 seconds)

Behavior Requirements:
- Begin rotation automatically after page load if all prerequisites are met
- Display one command at a time in a random, non-repeating sequence
- Ensure smooth transitions between commands
- Immediately stop rotation when:
  - User enters text in the search box
  - User applies any filter
  - User enables auto-filter
  - User interacts with any page element

Technical Considerations:
- Use requestAnimationFrame or setInterval for smooth animation
- Implement proper cleanup of event listeners and timers
- Handle edge cases (empty command list, single command)
- Ensure accessibility compliance
- Add appropriate loading states and error handling

 Note this is specifically about the TalonVoiceCommandSearch component  located in the TalonVoiceCommandsServer.Components.Pages namespace.

The information to be displayed should be the same as the full card of the search results.

 In addition to completing the requirements above also create a playwright test that takes a screenshot of the feature working and showing a random command on the screen.

 Please do not complete until this is actually working.

  please call the new branch that you will create "feature/auto-rotate-voice-commands"