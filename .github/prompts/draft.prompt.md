I would like to talon voice command results to include matches to the spoken form in talon lists table.

For example if I say "game left" I want to see results that match "left" in the spoken form column of the talon lists table, Given that there is a command with the following value: "game <user.arrow_key>".

Please provide a solution or guidance on how to achieve this.

The Blazor component is called TalonVoiceCommandSearch.razor.

Also note that the function  called SearchCommandNamesOnlyAsync  searches the whole term whereas it needs to search word otherwise it will never match so it needs to search for the word game and also the word left can you fix this for me ?