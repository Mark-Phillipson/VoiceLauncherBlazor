-- Migration: 20251226120000_MIGFixLanguages
-- Ensures Languages table columns and constraints match the model

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251226120000_MIGFixLanguages'
)
BEGIN
    -- Ensure Language column exists and is NVARCHAR(25) NOT NULL
    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Languages' AND COLUMN_NAME = 'Language')
    BEGIN
        ALTER TABLE [Languages] ADD [Language] nvarchar(25) NULL;
        UPDATE [Languages] SET [Language] = '' WHERE [Language] IS NULL;
        ALTER TABLE [Languages] ALTER COLUMN [Language] nvarchar(25) NOT NULL;
    END
    ELSE
    BEGIN
        IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Languages' AND COLUMN_NAME = 'Language' AND IS_NULLABLE = 'YES')
        BEGIN
            UPDATE [Languages] SET [Language] = '' WHERE [Language] IS NULL;
            ALTER TABLE [Languages] ALTER COLUMN [Language] nvarchar(25) NOT NULL;
        END
        DECLARE @currLen INT = (SELECT CHARACTER_MAXIMUM_LENGTH FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Languages' AND COLUMN_NAME = 'Language');
        IF @currLen IS NOT NULL AND @currLen <> 25
        BEGIN
            ALTER TABLE [Languages] ALTER COLUMN [Language] nvarchar(25) NOT NULL;
        END
    END

    -- Add Colour column if missing
    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Languages' AND COLUMN_NAME = 'Colour')
    BEGIN
        ALTER TABLE [Languages] ADD [Colour] nvarchar(40) NULL;
    END

    -- Add ImageLink column if missing
    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Languages' AND COLUMN_NAME = 'ImageLink')
    BEGIN
        ALTER TABLE [Languages] ADD [ImageLink] nvarchar(200) NULL;
    END

    -- Add unique index on Language if safe (no duplicates)
    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'UX_Languages_Language' AND object_id = OBJECT_ID(N'Languages'))
    BEGIN
        IF NOT EXISTS (
            SELECT [Language] FROM [Languages] WHERE [Language] IS NOT NULL GROUP BY [Language] HAVING COUNT(*) > 1
        )
        BEGIN
            CREATE UNIQUE INDEX UX_Languages_Language ON [Languages]([Language]);
        END
        ELSE
        BEGIN
            PRINT 'Skipping unique index UX_Languages_Language because duplicate Language rows exist. Resolve duplicates before creating the index.';
        END
    END

END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251226120000_MIGFixLanguages'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251226120000_MIGFixLanguages', N'10.0.0');
END;
