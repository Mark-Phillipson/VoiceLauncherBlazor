CREATE TABLE [dbo].[CustomIntelliSense] (
    [ID]             INT            IDENTITY (1, 1) NOT NULL,
    [LanguageID]     INT            NOT NULL,
    [Display_Value]  NVARCHAR (255) NOT NULL,
    [SendKeys_Value] NVARCHAR (MAX) NULL,
    [Command_Type]   NVARCHAR (255) NULL,
    [CategoryID]     INT            NOT NULL,
    [Remarks]        NVARCHAR (255) NULL,
    [Search]         NVARCHAR (MAX) NULL,
    [ComputerID]     INT            NULL,
    [DeliveryType]   NVARCHAR (30)  NOT NULL,
    CONSTRAINT [PK_dbo.CustomIntelliSense] PRIMARY KEY CLUSTERED ([ID] ASC),
    CONSTRAINT [FK_dbo.CustomIntelliSense_dbo.Categories_CategoryID] FOREIGN KEY ([CategoryID]) REFERENCES [dbo].[Categories] ([ID]),
    CONSTRAINT [FK_dbo.CustomIntelliSense_dbo.Computers_ComputerID] FOREIGN KEY ([ComputerID]) REFERENCES [dbo].[Computers] ([ID]),
    CONSTRAINT [FK_dbo.CustomIntelliSense_dbo.Languages_LanguageID] FOREIGN KEY ([LanguageID]) REFERENCES [dbo].[Languages] ([ID])
);


GO
CREATE NONCLUSTERED INDEX [IX_CategoryID]
    ON [dbo].[CustomIntelliSense]([CategoryID] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_ComputerID]
    ON [dbo].[CustomIntelliSense]([ComputerID] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_LanguageID]
    ON [dbo].[CustomIntelliSense]([LanguageID] ASC);

