CREATE TABLE [dbo].[Appointments] (
    [ID]              INT            IDENTITY (1, 1) NOT NULL,
    [AppointmentType] INT            NOT NULL,
    [StartDate]       DATETIME2 (7)  NOT NULL,
    [EndDate]         DATETIME2 (7)  NOT NULL,
    [Caption]         NVARCHAR (255) NULL,
    [AllDay]          BIT            CONSTRAINT [DF_Appointments_AllDay] DEFAULT ((0)) NOT NULL,
    [Location]        NVARCHAR (255) NULL,
    [Description]     NVARCHAR (255) NULL,
    [Label]           INT            CONSTRAINT [DF_Appointments_Label] DEFAULT ((1)) NOT NULL,
    [Status]          INT            CONSTRAINT [DF_Appointments_Status] DEFAULT ((1)) NOT NULL,
    [Recurrence]      NVARCHAR (255) NULL,
    CONSTRAINT [PK_Appointments] PRIMARY KEY CLUSTERED ([ID] ASC)
);

