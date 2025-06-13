-- Add OperatingSystem column to TalonVoiceCommands table
-- This script should be run manually if Entity Framework migrations are not available

-- Check if the column already exists before adding it
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'TalonVoiceCommands') AND name = 'OperatingSystem')
BEGIN
    ALTER TABLE TalonVoiceCommands
    ADD OperatingSystem NVARCHAR(50) NULL
    
    PRINT 'OperatingSystem column added successfully to TalonVoiceCommands table'
END
ELSE
BEGIN
    PRINT 'OperatingSystem column already exists in TalonVoiceCommands table'
END
