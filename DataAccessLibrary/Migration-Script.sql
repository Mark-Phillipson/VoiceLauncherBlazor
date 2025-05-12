BEGIN TRANSACTION;
ALTER TABLE [CustomIntelliSense] ADD [SelectWordFromRight] int NOT NULL DEFAULT 0;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20240804120432_SelectWordFromRight1', N'9.0.0');

ALTER TABLE [CustomIntelliSense] ADD [MoveCharactersLeft] int NOT NULL DEFAULT 0;

ALTER TABLE [CustomIntelliSense] ADD [SelectCharactersLeft] int NOT NULL DEFAULT 0;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20240805071414_MoveAndSelectCharactersLeft', N'9.0.0');

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

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20240818174055_Transactions', N'9.0.0');

DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Transactions]') AND [c].[name] = N'MoneyOut');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Transactions] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [Transactions] ALTER COLUMN [MoneyOut] decimal(10,2) NOT NULL;

DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Transactions]') AND [c].[name] = N'MoneyIn');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Transactions] DROP CONSTRAINT [' + @var1 + '];');
ALTER TABLE [Transactions] ALTER COLUMN [MoneyIn] decimal(10,2) NOT NULL;

DECLARE @var2 sysname;
SELECT @var2 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Transactions]') AND [c].[name] = N'Balance');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Transactions] DROP CONSTRAINT [' + @var2 + '];');
ALTER TABLE [Transactions] ALTER COLUMN [Balance] decimal(10,2) NOT NULL;

ALTER TABLE [Transactions] ADD [ImportDate] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';

ALTER TABLE [Transactions] ADD [ImportFilename] nvarchar(max) NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20240819092912_TransactionRevision1', N'9.0.0');

CREATE TABLE [TransactionTypeMapping] (
    [Id] int NOT NULL IDENTITY,
    [MyTransactionType] nvarchar(50) NOT NULL,
    [Type] nvarchar(50) NOT NULL,
    CONSTRAINT [PK_TransactionTypeMapping] PRIMARY KEY ([Id])
);

CREATE UNIQUE INDEX [IX_TransactionTypeMapping_MyTransactionType_Type] ON [TransactionTypeMapping] ([MyTransactionType], [Type]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20240819104323_TransactionMapping', N'9.0.0');

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250511170235_resetting', N'9.0.0');

COMMIT;
GO

