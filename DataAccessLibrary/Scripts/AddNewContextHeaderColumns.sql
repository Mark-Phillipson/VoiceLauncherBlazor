-- Migration script to add new Talon context header columns
-- CodeLanguage, Language, and Hostname fields

-- Check if columns don't exist before adding them
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'TalonVoiceCommands' AND COLUMN_NAME = 'CodeLanguage')
BEGIN
    ALTER TABLE TalonVoiceCommands 
    ADD CodeLanguage NVARCHAR(300) NULL;
    PRINT 'Added CodeLanguage column to TalonVoiceCommands table';
END
ELSE
BEGIN
    PRINT 'CodeLanguage column already exists in TalonVoiceCommands table';
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'TalonVoiceCommands' AND COLUMN_NAME = 'Language')
BEGIN
    ALTER TABLE TalonVoiceCommands 
    ADD Language NVARCHAR(300) NULL;
    PRINT 'Added Language column to TalonVoiceCommands table';
END
ELSE
BEGIN
    PRINT 'Language column already exists in TalonVoiceCommands table';
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'TalonVoiceCommands' AND COLUMN_NAME = 'Hostname')
BEGIN
    ALTER TABLE TalonVoiceCommands 
    ADD Hostname NVARCHAR(300) NULL;
    PRINT 'Added Hostname column to TalonVoiceCommands table';
END
ELSE
BEGIN
    PRINT 'Hostname column already exists in TalonVoiceCommands table';
END

-- Verify the columns were added
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'TalonVoiceCommands' 
    AND COLUMN_NAME IN ('CodeLanguage', 'Language', 'Hostname')
ORDER BY COLUMN_NAME;

PRINT 'Migration completed successfully. New context header columns added to TalonVoiceCommands table.';
