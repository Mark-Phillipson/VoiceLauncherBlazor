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
-  Note when dealing with index db we cannot use Blazor to fetch the data as it is non performant

 Note this is specifically about the TalonVoiceCommandSearch component  located in the TalonVoiceCommandsServer.Components.Pages namespace.

The information to be displayed should be the same as the full card of the search results  using the same badge colors for each category.

 In addition to completing the requirements above also create a playwright test that takes a screenshot of the feature working and showing a random command on the screen.

 Please do not complete until this is actually working.

  please call the new branch that you will create "feature/auto-rotate-voice-commands"

  Do not change the dotnet version it must stay at dotnet 9 and do not make any changes to any files other than in the project TalonVoiceCommandsServer.

  ---

  Further Details
  -------------------------------------------------

  1) IndexedDB helper API
    - Use the existing client-side helper exposed as `indexeddb-storage.js`.

  2) Command data shape
    - Each command object will have at least these fields (camelCase):
      - `talonVoiceCommandName` (string) Note this is what is said into the microphone for the command to be fired
      - `repository` (string)
      - `application` (array of strings)
      - `tags` (array of strings)
      - `script` (ISO date string)
      - `filename` (string)
      - `mode` (string)
      - `os` (string)
    - I'll only render fields that are present; missing optional fields will be omitted gracefully.

  3) Where the rotating card is rendered
    - Render the rotating single-card UI inside the existing `div.search-results-container` element in `TalonVoiceCommandSearch.razor`. That element currently is hidden (`display:none`) and will be toggled visible when rotation starts.

  4) Rotation timing and configuration
    - Default rotation interval: 7 seconds.
    -  there is no need to have this configurable we can just change the code if need be

  5) Start conditions (prerequisites)
    Rotation will begin automatically after the DOMContentLoaded event if all of the following are true:
    - Search text box is empty (detect `#searchTerm` value empty).
    - No active filters are applied (detect by checking that no filter inputs/selects inside the filters container have values or by querying the same filter state the razor component uses).
    - The Auto Filter by application setting is currently disabled. 

  6) Stop conditions (when rotation immediately stops)
    Rotation will stop and not auto-resume for the session (until page refresh) when any of the following happen:
    - User enters text in the search box (input event on `#searchTerm`).
    - User applies any filter (change events on inputs/selects inside the filters container).
    - User enables auto-filter (change event on `#autoFilterToggle` or equivalent control).
    - Any user interaction on the page (global `pointerdown` or `keydown` events). This is a first-interaction permanent stop behavior unless you ask for pause/resume instead.

  7) Detecting filters and auto-filter toggle
    - I'll listen for `change` events on all inputs/selects inside the filters area (the filters collapse container in the razor file). This keeps the implementation resilient to different control IDs.
    - I'll also look for an element with id `autoFilterToggle` (or a checkbox named similarly); if found I'll observe it specifically.

  8) Non-repeating random sequence and restart
    - The rotation will show each command once per cycle in a randomly shuffled order. After the cycle completes it will reshuffle and continue.

  9) Animation and rendering
    - Use CSS class-based opacity transitions (fade) and `requestAnimationFrame` for frame-accurate class toggles. Use `setInterval` for the logical rotation interval.
    - The rotating container will have `aria-live="polite"` and `role="status"` so screen readers are notified of content changes.

  10) Edge cases
    - Empty command list: show an accessible message inside the rotating container and do not start a timer.
    - Single command: show it without cross-fade (no repeated animation required) but keep accessible announcements.

  11) Cleanup
    - All event listeners and timers will be removed/cleared when rotation stops or when the page unloads.

  12) Styling
    - I'll reuse the existing card markup and Bootstrap classes so the rotating card visually matches the full cards shown in search results. Any small extra styles will be added in a new `wwwroot/css/auto-rotate.css` file scoped to the component.

  13) Playwright test
    - Test location: `PlaywrightTests/` (or the repo's Playwright test project). I'll add one test that:
      - Assumes the application is running at `http://localhost:5008` (this is the dev port used by this project per repo guidance).
      - Navigates to `/talon-voice-command-search`.
      - Waits for the rotating card to become visible and contain at least one command.
      - Takes a screenshot proving a random command is shown and saves it under the Playwright test artifacts.
    - The test will need to check to see if the server is already running and if not start it.

  14) Git/branch
    - I will create a branch named `feature/auto-rotate-voice-commands` for the changes and commit only files inside the `TalonVoiceCommandsServer` project (plus the Playwright test under `PlaywrightTests/`).

  15) Constraints
    - Do not change the global .NET version (keep .NET 9).
    - No new server-side endpoints will be added; all rotation data is fetched client-side from IndexedDB.
    - I will not add npm packages. Implementation will use plain vanilla JS and existing project assets.

  16) Tests and CI
    - Initially I'll add only the screenshot Playwright test (happy path). If you want additional tests (stop-on-input, filter-detection), I'll add them after the basic test is green.

  