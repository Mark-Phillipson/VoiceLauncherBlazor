-- Migration Script: Create ClubMembers table
IF OBJECT_ID('dbo.ClubMembers', 'U') IS NULL
BEGIN
CREATE TABLE [dbo].[ClubMembers]
(
    [Id] INT IDENTITY(1,1) PRIMARY KEY,
    [FirstName] NVARCHAR(100) NOT NULL,
    [LastName] NVARCHAR(100) NULL,
    [ImageData] VARBINARY(MAX) NULL,
    [ContentType] NVARCHAR(50) NULL,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT(GETDATE())
)
END
