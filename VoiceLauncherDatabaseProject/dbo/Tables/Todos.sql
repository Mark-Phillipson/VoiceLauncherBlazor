CREATE TABLE [dbo].[Todos] (
    [ID]          INT             IDENTITY (1, 1) NOT NULL,
    [Title]       NVARCHAR (255)  NOT NULL,
    [Description] NVARCHAR (1000) NOT NULL,
    [Completed]   BIT             CONSTRAINT [DF__Todos__Completed__160F4887] DEFAULT ((0)) NOT NULL,
    [Project]     NVARCHAR (255)  NULL,
    [Archived]    BIT             CONSTRAINT [DF_Todos_Archived] DEFAULT ((0)) NOT NULL,
    [Created] DATETIME2 NOT NULL DEFAULT GetDate(), 
    CONSTRAINT [PK_dbo.Todos] PRIMARY KEY CLUSTERED ([ID] ASC)
);

