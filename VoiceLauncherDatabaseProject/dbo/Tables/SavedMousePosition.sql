CREATE TABLE [dbo].[SavedMousePosition] (
    [ID]            INT            IDENTITY (1, 1) NOT NULL,
    [NamedLocation] NVARCHAR (255) NOT NULL,
    [X]             INT            NOT NULL,
    [Y]             INT            NOT NULL,
    [Created]       DATETIME       NULL,
    CONSTRAINT [PK_dbo.SavedMousePosition] PRIMARY KEY CLUSTERED ([ID] ASC)
);

