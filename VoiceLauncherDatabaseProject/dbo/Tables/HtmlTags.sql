CREATE TABLE [dbo].[HtmlTags] (
    [ID]          INT            IDENTITY (1, 1) NOT NULL,
    [Tag]         NVARCHAR (255) NULL,
    [Description] NVARCHAR (255) NULL,
    [ListValue]   NVARCHAR (255) NULL,
    [Include]     BIT            NOT NULL,
    [SpokenForm]  NVARCHAR (255) NULL,
    CONSTRAINT [PK_dbo.HtmlTags] PRIMARY KEY CLUSTERED ([ID] ASC)
);

