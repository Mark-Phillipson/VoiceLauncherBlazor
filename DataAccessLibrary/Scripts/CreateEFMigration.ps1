# Navigate to the project directory that contains your DbContext
cd c:\Users\MPhil\source\repos\VoiceLauncherBlazor\DataAccessLibrary

# Create a new migration
dotnet ef migrations add AddLauncherCategoryBridge --project DataAccessLibrary --startup-project ..\VoiceLauncherBlazor

# Apply the migration to the database
dotnet ef database update --project DataAccessLibrary --startup-project ..\VoiceLauncherBlazor
