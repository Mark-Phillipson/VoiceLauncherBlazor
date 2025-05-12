-- Fix migration issue where CursorlessCheatsheetItems table already exists
-- Check if YoutubeLink column exists, add if it doesn't

IF NOT EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'CursorlessCheatsheetItems' 
    AND COLUMN_NAME = 'YoutubeLink'
)
BEGIN
    -- Add the YoutubeLink column to existing table
    ALTER TABLE [CursorlessCheatsheetItems] 
    ADD [YoutubeLink] nvarchar(255) NULL;

    PRINT 'Added YoutubeLink column to CursorlessCheatsheetItems table';
END
ELSE
BEGIN
    PRINT 'YoutubeLink column already exists in CursorlessCheatsheetItems table';
END

-- Update the __EFMigrationsHistory table to mark this migration as applied
-- First check if the migration entry already exists
IF NOT EXISTS (
    SELECT 1 
    FROM [__EFMigrationsHistory] 
    WHERE [MigrationId] = '20240712153756_MIGYoutubeLink'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES ('20240712153756_MIGYoutubeLink', '7.0.5');

    PRINT 'Added migration entry to __EFMigrationsHistory table';
END
ELSE
BEGIN
    PRINT 'Migration 20240712153756_MIGYoutubeLink already exists in __EFMigrationsHistory table';
END