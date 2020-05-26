CREATE TABLE [dbo].[Logins] (
    [ID]          INT            IDENTITY (1, 1) NOT NULL,
    [Name]        NVARCHAR (30)  NOT NULL,
    [Username]    NVARCHAR (255) NOT NULL,
    [Password]    NVARCHAR (255) NULL,
    [Description] NVARCHAR (255) NULL,
    [Created] DATETIMEOFFSET NULL, 
    CONSTRAINT [PK_dbo.Logins] PRIMARY KEY CLUSTERED ([ID] ASC)
);

