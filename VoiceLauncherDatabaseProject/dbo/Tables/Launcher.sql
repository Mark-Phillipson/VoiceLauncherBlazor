CREATE TABLE [dbo].[Launcher] (
    [ID]          INT            IDENTITY (1, 1) NOT NULL,
    [Name]        NVARCHAR (50)  NOT NULL,
    [CommandLine] NVARCHAR (255) NULL,
    [CategoryID]  INT            NOT NULL,
    [ComputerID]  INT            NULL,
    CONSTRAINT [PK_dbo.Launcher] PRIMARY KEY CLUSTERED ([ID] ASC),
    CONSTRAINT [FK_dbo.Launcher_dbo.Categories_CategoryID] FOREIGN KEY ([CategoryID]) REFERENCES [dbo].[Categories] ([ID]) ON DELETE CASCADE,
    CONSTRAINT [FK_dbo.Launcher_dbo.Computers_ComputerID] FOREIGN KEY ([ComputerID]) REFERENCES [dbo].[Computers] ([ID])
);


GO
CREATE NONCLUSTERED INDEX [IX_CategoryID]
    ON [dbo].[Launcher]([CategoryID] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_ComputerID]
    ON [dbo].[Launcher]([ComputerID] ASC);

