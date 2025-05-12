IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20220731083258_NewCommandTable', N'9.0.0');

-- CREATE TABLE [CustomWindowsSpeechCommands] (
--     [Id] int NOT NULL IDENTITY,
--     [TextToEnter] nvarchar(100) NULL,
--     [KeyDownValue] int NULL,
--     [ModifierKey] int NULL,
--     [KeyPressValue] int NULL,
--     [MouseCommand] nvarchar(100) NULL,
--     [ProcessStart] nvarchar(100) NULL,
--     [CommandLineArguments] nvarchar(100) NULL,
--     [Description] nvarchar(1000) NULL,
--     CONSTRAINT [PK_CustomWindowsSpeechCommands] PRIMARY KEY ([Id])
-- );

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20220731084307_NewCommandTable2', N'9.0.0');

ALTER TABLE [CustomWindowsSpeechCommands] ADD [SpokenCommand] nvarchar(100) NOT NULL DEFAULT N'';

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20220731085225_CommandTableForgot', N'9.0.0');

DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[CustomWindowsSpeechCommands]') AND [c].[name] = N'Description');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [CustomWindowsSpeechCommands] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [CustomWindowsSpeechCommands] DROP COLUMN [Description];

DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[CustomWindowsSpeechCommands]') AND [c].[name] = N'SpokenCommand');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [CustomWindowsSpeechCommands] DROP CONSTRAINT [' + @var1 + '];');
ALTER TABLE [CustomWindowsSpeechCommands] DROP COLUMN [SpokenCommand];

ALTER TABLE [CustomWindowsSpeechCommands] ADD [WindowsSpeechVoiceCommandId] int NOT NULL DEFAULT 0;

CREATE TABLE [WindowsSpeechVoiceCommand] (
    [Id] int NOT NULL IDENTITY,
    [SpokenCommand] nvarchar(100) NOT NULL,
    [Description] nvarchar(1000) NULL,
    CONSTRAINT [PK_WindowsSpeechVoiceCommand] PRIMARY KEY ([Id])
);

CREATE INDEX [IX_CustomWindowsSpeechCommands_WindowsSpeechVoiceCommandId] ON [CustomWindowsSpeechCommands] ([WindowsSpeechVoiceCommandId]);

ALTER TABLE [CustomWindowsSpeechCommands] ADD CONSTRAINT [FK_CustomWindowsSpeechCommands_WindowsSpeechVoiceCommand_WindowsSpeechVoiceCommandId] FOREIGN KEY ([WindowsSpeechVoiceCommandId]) REFERENCES [WindowsSpeechVoiceCommand] ([Id]) ON DELETE CASCADE;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20220801070859_WSRNewTable1', N'9.0.0');

ALTER TABLE [WindowsSpeechVoiceCommand] ADD [ApplicationName] nvarchar(50) NULL;

ALTER TABLE [CustomWindowsSpeechCommands] ADD [AlternateKey] bit NOT NULL DEFAULT CAST(0 AS bit);

ALTER TABLE [CustomWindowsSpeechCommands] ADD [ControlKey] bit NOT NULL DEFAULT CAST(0 AS bit);

ALTER TABLE [CustomWindowsSpeechCommands] ADD [ShiftKey] bit NOT NULL DEFAULT CAST(0 AS bit);

ALTER TABLE [CustomWindowsSpeechCommands] ADD [WaitTime] int NOT NULL DEFAULT 0;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20220802083856_Moorefield1', N'9.0.0');

ALTER TABLE [CustomWindowsSpeechCommands] ADD [AbsoluteX] float NOT NULL DEFAULT 0.0E0;

ALTER TABLE [CustomWindowsSpeechCommands] ADD [AbsoluteY] float NOT NULL DEFAULT 0.0E0;

ALTER TABLE [CustomWindowsSpeechCommands] ADD [MouseMoveX] int NOT NULL DEFAULT 0;

ALTER TABLE [CustomWindowsSpeechCommands] ADD [MouseMoveY] int NOT NULL DEFAULT 0;

ALTER TABLE [CustomWindowsSpeechCommands] ADD [ScrollAmount] int NOT NULL DEFAULT 0;

ALTER TABLE [CustomWindowsSpeechCommands] ADD [WindowsKey] bit NOT NULL DEFAULT CAST(0 AS bit);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20220807061018_MouseCommandColumns1', N'9.0.0');

ALTER TABLE [CustomWindowsSpeechCommands] ADD [KeyUpValue] int NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20220907163904_MIGKeyUp', N'9.0.0');

CREATE TABLE [GrammarNames] (
    [Id] int NOT NULL IDENTITY,
    [NameOfGrammar] nvarchar(40) NOT NULL,
    CONSTRAINT [PK_GrammarNames] PRIMARY KEY ([Id])
);

CREATE TABLE [GrammarItems] (
    [Id] int NOT NULL IDENTITY,
    [GrammarNameId] int NOT NULL,
    [Value] nvarchar(60) NOT NULL,
    CONSTRAINT [PK_GrammarItems] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_GrammarItems_GrammarNames_GrammarNameId] FOREIGN KEY ([GrammarNameId]) REFERENCES [GrammarNames] ([Id]) ON DELETE CASCADE
);

CREATE INDEX [IX_GrammarItems_GrammarNameId] ON [GrammarItems] ([GrammarNameId]);

CREATE UNIQUE INDEX [IX_GrammarNames_NameOfGrammar] ON [GrammarNames] ([NameOfGrammar]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20220908082739_MIGGrammars', N'9.0.0');

ALTER TABLE [WindowsSpeechVoiceCommand] ADD [AutoCreated] bit NOT NULL DEFAULT CAST(0 AS bit);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20220918075853_MIGAutoCreated', N'9.0.0');

ALTER TABLE [CustomWindowsSpeechCommands] ADD [SendKeysValue] nvarchar(40) NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20220918104645_MIGSendKeysValue', N'9.0.0');

DECLARE @var2 sysname;
SELECT @var2 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[CustomWindowsSpeechCommands]') AND [c].[name] = N'ProcessStart');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [CustomWindowsSpeechCommands] DROP CONSTRAINT [' + @var2 + '];');
ALTER TABLE [CustomWindowsSpeechCommands] ALTER COLUMN [ProcessStart] nvarchar(255) NULL;

DECLARE @var3 sysname;
SELECT @var3 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[CustomWindowsSpeechCommands]') AND [c].[name] = N'CommandLineArguments');
IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [CustomWindowsSpeechCommands] DROP CONSTRAINT [' + @var3 + '];');
ALTER TABLE [CustomWindowsSpeechCommands] ALTER COLUMN [CommandLineArguments] nvarchar(255) NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20221103102120_MIGStartProcessBigger', N'9.0.0');

DECLARE @var4 sysname;
SELECT @var4 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[CustomWindowsSpeechCommands]') AND [c].[name] = N'TextToEnter');
IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [CustomWindowsSpeechCommands] DROP CONSTRAINT [' + @var4 + '];');
ALTER TABLE [CustomWindowsSpeechCommands] ALTER COLUMN [TextToEnter] nvarchar(255) NULL;

ALTER TABLE [CustomWindowsSpeechCommands] ADD [HowToFormatDictation] nvarchar(55) NULL;

ALTER TABLE [CustomWindowsSpeechCommands] ADD [MethodToCall] nvarchar(255) NULL;

CREATE TABLE [ApplicationDetails] (
    [Id] int NOT NULL IDENTITY,
    [ProcessName] nvarchar(60) NOT NULL,
    [ApplicationTitle] nvarchar(255) NOT NULL,
    CONSTRAINT [PK_ApplicationDetails] PRIMARY KEY ([Id])
);

CREATE TABLE [Idiosyncrasies] (
    [Id] int NOT NULL IDENTITY,
    [FindString] nvarchar(60) NULL,
    [ReplaceWith] nvarchar(60) NULL,
    CONSTRAINT [PK_Idiosyncrasies] PRIMARY KEY ([Id])
);

CREATE TABLE [PhraseListGrammars] (
    [Id] int NOT NULL IDENTITY,
    [PhraseListGrammarValue] nvarchar(100) NOT NULL,
    CONSTRAINT [PK_PhraseListGrammars] PRIMARY KEY ([Id])
);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20221114120002_MIGNewTables', N'9.0.0');

CREATE TABLE [CssProperties] (
    [Id] int NOT NULL IDENTITY,
    [PropertyName] nvarchar(100) NOT NULL,
    [Description] nvarchar(255) NULL,
    CONSTRAINT [PK_CssProperties] PRIMARY KEY ([Id])
);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20221219091116_MIGCssProperties', N'9.0.0');

ALTER TABLE [Languages] ADD [Colour] nvarchar(40) NULL;

ALTER TABLE [Categories] ADD [Colour] nvarchar(40) NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20221224171747_MIGColours', N'9.0.0');

ALTER TABLE [Launcher] ADD [Colour] nvarchar(30) NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20221227143624_MIGLauncherColour', N'9.0.0');

ALTER TABLE [Launcher] ADD [Icon] nvarchar(100) NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230409061100_MIGIcon', N'9.0.0');

DECLARE @var5 sysname;
SELECT @var5 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Launcher]') AND [c].[name] = N'Icon');
IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [Launcher] DROP CONSTRAINT [' + @var5 + '];');
ALTER TABLE [Launcher] ALTER COLUMN [Icon] nvarchar(100) NULL;

CREATE TABLE [SpokenForm] (
    [Id] int NOT NULL IDENTITY,
    [SpokenFormText] nvarchar(100) NOT NULL,
    [WindowsSpeechVoiceCommandId] int NOT NULL,
    CONSTRAINT [PK_SpokenForm] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_SpokenForm_WindowsSpeechVoiceCommand_WindowsSpeechVoiceCommandId] FOREIGN KEY ([WindowsSpeechVoiceCommandId]) REFERENCES [WindowsSpeechVoiceCommand] ([Id]) ON DELETE CASCADE
);

CREATE UNIQUE INDEX [IX_SpokenForm_SpokenFormText_WindowsSpeechVoiceCommandId] ON [SpokenForm] ([SpokenFormText], [WindowsSpeechVoiceCommandId]);

CREATE INDEX [IX_SpokenForm_WindowsSpeechVoiceCommandId] ON [SpokenForm] ([WindowsSpeechVoiceCommandId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230416121119_MIGSpokenForms', N'9.0.0');

DECLARE @var6 sysname;
SELECT @var6 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[WindowsSpeechVoiceCommand]') AND [c].[name] = N'SpokenCommand');
IF @var6 IS NOT NULL EXEC(N'ALTER TABLE [WindowsSpeechVoiceCommand] DROP CONSTRAINT [' + @var6 + '];');
ALTER TABLE [WindowsSpeechVoiceCommand] ALTER COLUMN [SpokenCommand] nvarchar(max) NULL;

ALTER TABLE [Launcher] ADD [Favourite] bit NOT NULL DEFAULT CAST(0 AS bit);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230427052317_MIGFavoriteLaunchers', N'9.0.0');

ALTER TABLE [Idiosyncrasies] ADD [StringFormattingMethod] nvarchar(60) NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230430083334_MIGStringFormattingMethod', N'9.0.0');

ALTER TABLE [Categories] ADD [Icon] nvarchar(50) NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230430165707_MIGCategoryIcon', N'9.0.0');

CREATE TABLE [Microphones] (
    [Id] int NOT NULL IDENTITY,
    [MicrophoneName] nvarchar(100) NULL,
    [Default] bit NOT NULL,
    CONSTRAINT [PK_Microphones] PRIMARY KEY ([Id])
);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230507090455_MIGMicrophone', N'9.0.0');

DECLARE @var7 sysname;
SELECT @var7 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Microphones]') AND [c].[name] = N'MicrophoneName');
IF @var7 IS NOT NULL EXEC(N'ALTER TABLE [Microphones] DROP CONSTRAINT [' + @var7 + '];');
UPDATE [Microphones] SET [MicrophoneName] = N'' WHERE [MicrophoneName] IS NULL;
ALTER TABLE [Microphones] ALTER COLUMN [MicrophoneName] nvarchar(100) NOT NULL;
ALTER TABLE [Microphones] ADD DEFAULT N'' FOR [MicrophoneName];

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230507092115_MIGMigration2', N'9.0.0');

CREATE TABLE [Prompts] (
    [Id] int NOT NULL IDENTITY,
    [PromptText] nvarchar(200) NOT NULL,
    [Description] nvarchar(2000) NULL,
    [IsDefault] bit NOT NULL,
    CONSTRAINT [PK_Prompts] PRIMARY KEY ([Id])
);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230526101738_MIGPrompt', N'9.0.0');

DECLARE @var8 sysname;
SELECT @var8 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Prompts]') AND [c].[name] = N'PromptText');
IF @var8 IS NOT NULL EXEC(N'ALTER TABLE [Prompts] DROP CONSTRAINT [' + @var8 + '];');
ALTER TABLE [Prompts] ALTER COLUMN [PromptText] nvarchar(900) NOT NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230526110859_MIGPrompt1', N'9.0.0');

DECLARE @var9 sysname;
SELECT @var9 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Prompts]') AND [c].[name] = N'PromptText');
IF @var9 IS NOT NULL EXEC(N'ALTER TABLE [Prompts] DROP CONSTRAINT [' + @var9 + '];');
ALTER TABLE [Prompts] ALTER COLUMN [PromptText] nvarchar(3000) NOT NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230528085119_MIGPromptTextLength', N'9.0.0');

ALTER TABLE [Launcher] ADD [Arguments] nvarchar(255) NULL;

ALTER TABLE [Launcher] ADD [WorkingDirectory] nvarchar(255) NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230604091706_MIGLauncherArguments', N'9.0.0');

ALTER TABLE [CustomIntelliSense] ADD [Variable1] nvarchar(60) NULL;

ALTER TABLE [CustomIntelliSense] ADD [Variable2] nvarchar(60) NULL;

ALTER TABLE [CustomIntelliSense] ADD [Variable3] nvarchar(60) NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230923082201_MIGVariables', N'9.0.0');

DECLARE @var10 sysname;
SELECT @var10 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[WindowsSpeechVoiceCommand]') AND [c].[name] = N'SpokenCommand');
IF @var10 IS NOT NULL EXEC(N'ALTER TABLE [WindowsSpeechVoiceCommand] DROP CONSTRAINT [' + @var10 + '];');
UPDATE [WindowsSpeechVoiceCommand] SET [SpokenCommand] = N'' WHERE [SpokenCommand] IS NULL;
ALTER TABLE [WindowsSpeechVoiceCommand] ALTER COLUMN [SpokenCommand] nvarchar(100) NOT NULL;
ALTER TABLE [WindowsSpeechVoiceCommand] ADD DEFAULT N'' FOR [SpokenCommand];

DECLARE @var11 sysname;
SELECT @var11 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[WindowsSpeechVoiceCommand]') AND [c].[name] = N'ApplicationName');
IF @var11 IS NOT NULL EXEC(N'ALTER TABLE [WindowsSpeechVoiceCommand] DROP CONSTRAINT [' + @var11 + '];');
ALTER TABLE [WindowsSpeechVoiceCommand] ALTER COLUMN [ApplicationName] nvarchar(max) NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20231126132634_MIGTalonAlphabet', N'9.0.0');

CREATE TABLE [TalonAlphabets] (
    [Id] int NOT NULL IDENTITY,
    [Letter] nvarchar(1) NOT NULL,
    [PictureUrl] nvarchar(255) NULL,
    [DefaultLetter] nvarchar(1) NOT NULL,
    [DefaultPictureUrl] nvarchar(255) NULL,
    CONSTRAINT [PK_TalonAlphabets] PRIMARY KEY ([Id])
);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20231126134550_MIGTalonAlphabet1', N'9.0.0');

DECLARE @var12 sysname;
SELECT @var12 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[TalonAlphabets]') AND [c].[name] = N'Letter');
IF @var12 IS NOT NULL EXEC(N'ALTER TABLE [TalonAlphabets] DROP CONSTRAINT [' + @var12 + '];');
ALTER TABLE [TalonAlphabets] ALTER COLUMN [Letter] nvarchar(20) NOT NULL;

DECLARE @var13 sysname;
SELECT @var13 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[TalonAlphabets]') AND [c].[name] = N'DefaultLetter');
IF @var13 IS NOT NULL EXEC(N'ALTER TABLE [TalonAlphabets] DROP CONSTRAINT [' + @var13 + '];');
ALTER TABLE [TalonAlphabets] ALTER COLUMN [DefaultLetter] nvarchar(20) NOT NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20231126141814_MIGTalonAlphabet2', N'9.0.0');

ALTER TABLE [Launcher] ADD [SortOrder] int NOT NULL DEFAULT 0;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20240120161515_MIGSortOrder', N'9.0.0');

CREATE TABLE [CursorlessCheatsheetItems] (
    [Id] int NOT NULL IDENTITY,
    [SpokenForm] nvarchar(max) NOT NULL,
    [Meaning] nvarchar(max) NULL,
    [CursorlessType] nvarchar(max) NULL,
    [YoutubeLink] nvarchar(255) NULL,
    CONSTRAINT [PK_CursorlessCheatsheetItems] PRIMARY KEY ([Id])
);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20240712153756_MIGYoutubeLink', N'9.0.0');

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

DECLARE @var14 sysname;
SELECT @var14 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Transactions]') AND [c].[name] = N'MoneyOut');
IF @var14 IS NOT NULL EXEC(N'ALTER TABLE [Transactions] DROP CONSTRAINT [' + @var14 + '];');
ALTER TABLE [Transactions] ALTER COLUMN [MoneyOut] decimal(10,2) NOT NULL;

DECLARE @var15 sysname;
SELECT @var15 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Transactions]') AND [c].[name] = N'MoneyIn');
IF @var15 IS NOT NULL EXEC(N'ALTER TABLE [Transactions] DROP CONSTRAINT [' + @var15 + '];');
ALTER TABLE [Transactions] ALTER COLUMN [MoneyIn] decimal(10,2) NOT NULL;

DECLARE @var16 sysname;
SELECT @var16 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Transactions]') AND [c].[name] = N'Balance');
IF @var16 IS NOT NULL EXEC(N'ALTER TABLE [Transactions] DROP CONSTRAINT [' + @var16 + '];');
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

CREATE TABLE [LauncherCategoryBridge] (
    [ID] int NOT NULL IDENTITY,
    [LauncherID] int NOT NULL,
    [CategoryID] int NOT NULL,
    CONSTRAINT [PK_LauncherCategoryBridge] PRIMARY KEY ([ID]),
    CONSTRAINT [FK_LauncherCategoryBridge_Categories_CategoryID] FOREIGN KEY ([CategoryID]) REFERENCES [Categories] ([ID]) ON DELETE CASCADE,
    CONSTRAINT [FK_LauncherCategoryBridge_Launcher_LauncherID] FOREIGN KEY ([LauncherID]) REFERENCES [Launcher] ([ID]) ON DELETE CASCADE
);

CREATE INDEX [IX_LauncherCategoryBridge_CategoryID] ON [LauncherCategoryBridge] ([CategoryID]);

CREATE INDEX [IX_LauncherCategoryBridge_LauncherID] ON [LauncherCategoryBridge] ([LauncherID]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250512072028_LanguageCategoryBridge', N'9.0.0');

COMMIT;
GO

