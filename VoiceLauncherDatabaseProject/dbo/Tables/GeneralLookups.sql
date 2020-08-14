CREATE TABLE [dbo].[GeneralLookups] (
    [ID]           INT            IDENTITY (1, 1) NOT NULL,
    [Item_Value]   NVARCHAR (255) NOT NULL,
    [Category]     NVARCHAR (255) NOT NULL,
    [SortOrder]    INT            NULL,
    [DisplayValue] NVARCHAR (255) NULL,
    CONSTRAINT [PK_dbo.GeneralLookups] PRIMARY KEY CLUSTERED ([ID] ASC)
);

