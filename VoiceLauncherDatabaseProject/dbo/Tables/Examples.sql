CREATE TABLE [dbo].[Examples] (
    [ID]          INT            IDENTITY (1, 1) NOT NULL,
    [NumberValue] INT            NOT NULL,
    [Text]        NVARCHAR (255) NOT NULL,
    [LargeText]   NVARCHAR (MAX) NOT NULL,
    [Boolean]     BIT            CONSTRAINT [DF__Examples__Boolea__03F0984C] DEFAULT ((0)) NOT NULL,
    [DateValue]   DATETIME2 (7)  NULL,
    CONSTRAINT [PK_dbo.Examples] PRIMARY KEY CLUSTERED ([ID] ASC)
);

