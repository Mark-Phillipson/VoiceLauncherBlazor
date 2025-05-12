-- Custom migration script to safely apply pending migrations
-- This checks for existence of columns before trying to create them

-- Check if migration has already been applied
IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20240712153756_MIGYoutubeLink')
BEGIN
    -- Migration 20240712153756_MIGYoutubeLink
    -- This is already handled by our previous script - the table already exists
    
    -- Mark the migration as applied
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20240712153756_MIGYoutubeLink', N'9.0.0');
    
    PRINT 'Applied 20240712153756_MIGYoutubeLink migration';
END
ELSE
BEGIN
    PRINT 'Migration 20240712153756_MIGYoutubeLink already applied';
END

-- SelectWordFromRight1 migration
IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20240804120432_SelectWordFromRight1')
BEGIN
    -- Check if column exists
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'SelectWordFromRight' AND Object_ID = Object_ID(N'CustomIntelliSense'))
    BEGIN
        ALTER TABLE [CustomIntelliSense] ADD [SelectWordFromRight] int NOT NULL DEFAULT 0;
        PRINT 'Added SelectWordFromRight column';
    END
    ELSE
    BEGIN
        PRINT 'Column SelectWordFromRight already exists';
    END
    
    -- Mark migration as applied
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20240804120432_SelectWordFromRight1', N'9.0.0');
    
    PRINT 'Applied 20240804120432_SelectWordFromRight1 migration';
END
ELSE
BEGIN
    PRINT 'Migration 20240804120432_SelectWordFromRight1 already applied';
END

-- MoveAndSelectCharactersLeft migration
IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20240805071414_MoveAndSelectCharactersLeft')
BEGIN
    -- Check if column exists
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'MoveCharactersLeft' AND Object_ID = Object_ID(N'CustomIntelliSense'))
    BEGIN
        ALTER TABLE [CustomIntelliSense] ADD [MoveCharactersLeft] int NOT NULL DEFAULT 0;
        PRINT 'Added MoveCharactersLeft column';
    END
    ELSE
    BEGIN
        PRINT 'Column MoveCharactersLeft already exists';
    END
    
    -- Check if column exists
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'SelectCharactersLeft' AND Object_ID = Object_ID(N'CustomIntelliSense'))
    BEGIN
        ALTER TABLE [CustomIntelliSense] ADD [SelectCharactersLeft] int NOT NULL DEFAULT 0;
        PRINT 'Added SelectCharactersLeft column';
    END
    ELSE
    BEGIN
        PRINT 'Column SelectCharactersLeft already exists';
    END
    
    -- Mark migration as applied
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20240805071414_MoveAndSelectCharactersLeft', N'9.0.0');
    
    PRINT 'Applied 20240805071414_MoveAndSelectCharactersLeft migration';
END
ELSE
BEGIN
    PRINT 'Migration 20240805071414_MoveAndSelectCharactersLeft already applied';
END

-- Transactions migration
IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20240818174055_Transactions')
BEGIN
    -- Check if table exists
    IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Transactions]') AND type in (N'U'))
    BEGIN
        CREATE TABLE [Transactions] (
            [Id] int NOT NULL IDENTITY,
            [Date] datetime2 NOT NULL,
            [Description] nvarchar(150) NULL,
            [Type] nvarchar(70) NULL,
            [MoneyIn] decimal(10,2) NOT NULL,
            [MoneyOut] decimal(10,2) NOT NULL,
            [Balance] decimal(10,2) NOT NULL,
            [MyTransactionType] nvarchar(70) NULL,
            CONSTRAINT [PK_Transactions] PRIMARY KEY ([Id])
        );
        PRINT 'Created Transactions table';
    END
    ELSE
    BEGIN
        PRINT 'Table Transactions already exists';
    END
    
    -- Mark migration as applied
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20240818174055_Transactions', N'9.0.0');
    
    PRINT 'Applied 20240818174055_Transactions migration';
END
ELSE
BEGIN
    PRINT 'Migration 20240818174055_Transactions already applied';
END

-- TransactionRevision1 migration
IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20240819092912_TransactionRevision1')
BEGIN
    -- Check if column exists
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'ImportDate' AND Object_ID = Object_ID(N'Transactions'))
    BEGIN
        ALTER TABLE [Transactions] ADD [ImportDate] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';
        PRINT 'Added ImportDate column';
    END
    ELSE
    BEGIN
        PRINT 'Column ImportDate already exists';
    END
    
    -- Check if column exists
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'ImportFilename' AND Object_ID = Object_ID(N'Transactions'))
    BEGIN
        ALTER TABLE [Transactions] ADD [ImportFilename] nvarchar(max) NULL;
        PRINT 'Added ImportFilename column';
    END
    ELSE
    BEGIN
        PRINT 'Column ImportFilename already exists';
    END
    
    -- Mark migration as applied
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20240819092912_TransactionRevision1', N'9.0.0');
    
    PRINT 'Applied 20240819092912_TransactionRevision1 migration';
END
ELSE
BEGIN
    PRINT 'Migration 20240819092912_TransactionRevision1 already applied';
END

-- TransactionMapping migration
IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20240819104323_TransactionMapping')
BEGIN
    -- Check if table exists
    IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TransactionTypeMapping]') AND type in (N'U'))
    BEGIN
        CREATE TABLE [TransactionTypeMapping] (
            [Id] int NOT NULL IDENTITY,
            [MyTransactionType] nvarchar(50) NOT NULL,
            [Type] nvarchar(50) NOT NULL,
            CONSTRAINT [PK_TransactionTypeMapping] PRIMARY KEY ([Id])
        );
        
        CREATE UNIQUE INDEX [IX_TransactionTypeMapping_MyTransactionType_Type] ON [TransactionTypeMapping] ([MyTransactionType], [Type]);
        PRINT 'Created TransactionTypeMapping table';
    END
    ELSE
    BEGIN
        PRINT 'Table TransactionTypeMapping already exists';
    END
    
    -- Mark migration as applied
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20240819104323_TransactionMapping', N'9.0.0');
    
    PRINT 'Applied 20240819104323_TransactionMapping migration';
END
ELSE
BEGIN
    PRINT 'Migration 20240819104323_TransactionMapping already applied';
END

-- resetting migration
IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20250511170235_resetting')
BEGIN
    -- This appears to be an empty migration, just mark it as applied
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250511170235_resetting', N'9.0.0');
    
    PRINT 'Applied 20250511170235_resetting migration';
END
ELSE
BEGIN
    PRINT 'Migration 20250511170235_resetting already applied';
END

PRINT 'All migrations have been applied!';
