CREATE TABLE [dbo].[Categories] (
    [ID]            INT            IDENTITY (1, 1) NOT NULL,
    [Category]      NVARCHAR (30)  NULL,
    [Category_Type] NVARCHAR (255) NULL,
    [Sensitive]     BIT            CONSTRAINT [DF_Categories_Sensitive] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_dbo.Categories] PRIMARY KEY CLUSTERED ([ID] ASC)
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Categories]
    ON [dbo].[Categories]([Category] ASC, [Category_Type] ASC);

