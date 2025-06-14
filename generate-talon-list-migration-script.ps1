# Generate TalonList Migration SQL Script
# This command generates a SQL script for the TalonList table migration
# Run this from the DataAccessLibrary directory

# Change to the DataAccessLibrary directory
cd "c:\Users\MPhil\source\repos\VoiceLauncherBlazor\DataAccessLibrary"

# Generate the SQL script for the migration
dotnet ef migrations script --configuration Debug --output "Migration-Script-TalonList.sql"
