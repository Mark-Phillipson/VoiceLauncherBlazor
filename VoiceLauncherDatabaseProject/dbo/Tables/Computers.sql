CREATE TABLE [dbo].[Computers] (
    [ID]           INT           IDENTITY (1, 1) NOT NULL,
    [ComputerName] NVARCHAR (20) NOT NULL,
    CONSTRAINT [PK_dbo.Computers] PRIMARY KEY CLUSTERED ([ID] ASC)
);

