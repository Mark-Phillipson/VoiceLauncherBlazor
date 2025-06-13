-- Migration Script: Add TalonVoiceCommands Table with String Lengths (EF Core)

IF OBJECT_ID('[dbo].[TalonVoiceCommands]', 'U') IS NOT NULL
    DROP TABLE [dbo].[TalonVoiceCommands];

CREATE TABLE [dbo].[TalonVoiceCommands] (
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [Command] NVARCHAR(100) NOT NULL,
    [Script] NVARCHAR(1000) NOT NULL,
    [Application] NVARCHAR(100) NOT NULL DEFAULT 'global',
    [Mode] NVARCHAR(100) NULL,
    [FilePath] NVARCHAR(250) NOT NULL,
    [CreatedAt] DATETIME2 NOT NULL
);

-- Optional: Add indexes if needed
-- CREATE INDEX IX_TalonVoiceCommands_Application ON [dbo].[TalonVoiceCommands]([Application]);
-- CREATE INDEX IX_TalonVoiceCommands_FilePath ON [dbo].[TalonVoiceCommands]([FilePath]);
