CREATE TABLE [dbo].[Languages] (
    [ID]       INT           IDENTITY (1, 1) NOT NULL,
    [Language] NVARCHAR (25) NOT NULL,
    [Active]   BIT           NOT NULL,
    CONSTRAINT [PK_dbo.Languages] PRIMARY KEY CLUSTERED ([ID] ASC)
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Languages]
    ON [dbo].[Languages]([Language] ASC);

