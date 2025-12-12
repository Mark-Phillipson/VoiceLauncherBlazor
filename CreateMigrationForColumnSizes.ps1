# Entity Framework Migration Commands
# Run these commands to update the database schema for larger column sizes

# Navigate to the DataAccessLibrary project
cd "c:\Users\MPhil\source\repos\VoiceLauncherBlazor\DataAccessLibrary"

# Add migration for larger column sizes
dotnet ef migrations add IncreaseColumnSizes --startup-project ..\VoiceAdmin

# Generate SQL script for the migration (optional - for review)
dotnet ef migrations script --startup-project ..\VoiceAdmin --output Migration-Script-IncreaseColumnSizes.sql

# Apply the migration to update the database
dotnet ef database update --startup-project ..\VoiceAdmin
