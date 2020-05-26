CREATE TABLE [dbo].[PropertyTabPositions] (
    [ID]           INT           IDENTITY (1, 1) NOT NULL,
    [ObjectName]   NVARCHAR (60) NOT NULL,
    [PropertyName] NVARCHAR (60) NOT NULL,
    [NumberOfTabs] INT           NOT NULL,
    CONSTRAINT [PK_dbo.PropertyTabPositions] PRIMARY KEY CLUSTERED ([ID] ASC)
);

