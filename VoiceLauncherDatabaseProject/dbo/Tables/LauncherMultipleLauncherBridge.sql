CREATE TABLE [dbo].[LauncherMultipleLauncherBridge] (
    [ID]                 INT IDENTITY (1, 1) NOT NULL,
    [LauncherID]         INT NOT NULL,
    [MultipleLauncherID] INT NOT NULL,
    CONSTRAINT [PK_dbo.LauncherMultipleLauncherBridge] PRIMARY KEY CLUSTERED ([ID] ASC),
    CONSTRAINT [FK_LauncherMultipleLauncherBridge_Launcher] FOREIGN KEY ([LauncherID]) REFERENCES [dbo].[Launcher] ([ID]),
    CONSTRAINT [FK_LauncherMultipleLauncherBridge_MultipleLauncher] FOREIGN KEY ([MultipleLauncherID]) REFERENCES [dbo].[MultipleLauncher] ([ID]) ON DELETE CASCADE
);

