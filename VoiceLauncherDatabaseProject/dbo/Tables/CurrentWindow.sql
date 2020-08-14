CREATE TABLE [dbo].[CurrentWindow] (
    [ID]     INT IDENTITY (1, 1) NOT NULL,
    [Handle] INT NOT NULL,
    CONSTRAINT [PK_dbo.CurrentWindow] PRIMARY KEY CLUSTERED ([ID] ASC)
);

