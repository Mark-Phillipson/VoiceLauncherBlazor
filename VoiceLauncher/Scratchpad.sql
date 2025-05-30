 SELECT * FROM Categories WHERE Id =4132
 SELECT * FROM Launcher  ORDER BY ID desc 
  DELETE FROM Launcher WHERE ID=32940
 SELECT * FROM LauncherCategoryBridge WHERE   LauncherId=32940
  DELETE FROM LauncherCategoryBridge WHERE ID=356
 UPDATE Launcher  set Arguments=Commandline
  WHERE CategoryId=4115
   UPDATE Launcher set Commandline='C:\Users\MPhil\AppData\Local\Programs\Microsoft VS Code\Code.exe'
   WHERE CategoryId=4115