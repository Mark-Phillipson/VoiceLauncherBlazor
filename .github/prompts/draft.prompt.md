The talon voice command search works but only directly after I've done an input if I refresh the page and then try it has no results does it need to load up the commands first? If so can you add that functionality?


Yes, the TalonVoiceCommandSearch component needs to load the commands when it is initialized to ensure that search functionality works correctly even after a page refresh. To achieve this, we can add a method to fetch and cache the commands when the component is first rendered.

Also, can we show the total number of commands and total number of lists currently in the system along with a list of repositories and the quantities for each?