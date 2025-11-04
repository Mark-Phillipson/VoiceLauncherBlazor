Is it possible in a Blazor webapp to have a process where we can ask the user to select a image file and then upload it to the images folder in the WWWRoot directory? If so, can you provide a simple example of how to implement this functionality?


 curl -X POST https://localhost:7264/api/uploads -F "file=@C:\Users\MPhil\OneDrive\Pictures\SearchTalonCommandsExtension.png" -k