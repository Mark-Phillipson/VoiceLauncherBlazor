CREATE TABLE [dbo].[ValuesToInsert] (
    [ID]            INT            IDENTITY (1, 1) NOT NULL,
    [ValueToInsert] NVARCHAR (255) NOT NULL,
    [Lookup]        NVARCHAR (255) NOT NULL,
    [Description]   NVARCHAR (255) NULL,
    CONSTRAINT [PK_dbo.ValuesToInsert] PRIMARY KEY CLUSTERED ([ID] ASC)
);

