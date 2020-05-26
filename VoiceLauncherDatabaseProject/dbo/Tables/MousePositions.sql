CREATE TABLE [dbo].[MousePositions] (
    [ID]          INT            IDENTITY (1, 1) NOT NULL,
    [Command]     NVARCHAR (255) NOT NULL,
    [MouseLeft]   INT            NOT NULL,
    [MouseTop]    INT            NOT NULL,
    [TabPageName] NVARCHAR (255) NULL,
    [ControlName] NVARCHAR (255) NULL,
    [Application] NVARCHAR (255) NULL,
    CONSTRAINT [PK_dbo.MousePositions] PRIMARY KEY CLUSTERED ([ID] ASC)
);

