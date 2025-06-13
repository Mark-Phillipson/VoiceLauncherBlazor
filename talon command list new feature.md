I would like to create a new feature where we can search the following folder and any sub folders for files ending in *.talon And pulling out the voice command and additional lines the voice command is always proceeded by a :  and anything after it is the script also some files have a heading Delimited by a - and in the header it lists what application or mode et that the voice command will run in. the end goal is to be able to  do a semantic search and return all results including the command and the scripts.

For more information on the talon file format see the following link https://talonvoice.com/docs/

C:\Users\MPhil\AppData\Roaming\talon\user

 so we would need a new table called  talon voice commands with the following fields:
- id (primary key)
- command (text) required
- script (text) required
- application (text) required (if non supplied default to global)
- Mode (text) (For example mixed, command, dictation, sleep)
- file_path (text) required
- created_at (timestamp) required

task one  create a model and a migration
task two  create a method to do the file search and fill in the table
task three create a Blazor component to perform the semantic search and display the results