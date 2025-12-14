-- Migration script for Face Recognition feature
-- This script creates the FaceImages and FaceTags tables

-- Create FaceImages table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[FaceImages]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[FaceImages] (
        [ID] INT IDENTITY(1,1) NOT NULL,
        [ImageName] NVARCHAR(200) NOT NULL,
        [Description] NVARCHAR(500) NULL,
        [ImageData] NVARCHAR(MAX) NOT NULL,
        [ContentType] NVARCHAR(50) NULL,
        [UploadDate] DATETIME NOT NULL DEFAULT GETDATE(),
        CONSTRAINT [PK_FaceImages] PRIMARY KEY CLUSTERED ([ID] ASC)
    );
    PRINT 'Created FaceImages table';
END
ELSE
BEGIN
    PRINT 'FaceImages table already exists';
END
GO

-- Create FaceTags table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[FaceTags]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[FaceTags] (
        [ID] INT IDENTITY(1,1) NOT NULL,
        [FaceImageId] INT NOT NULL,
        [FirstName] NVARCHAR(100) NOT NULL,
        [X] FLOAT NOT NULL,
        [Y] FLOAT NOT NULL,
        [Width] FLOAT NOT NULL,
        [Height] FLOAT NOT NULL,
        CONSTRAINT [PK_FaceTags] PRIMARY KEY CLUSTERED ([ID] ASC),
        CONSTRAINT [FK_FaceTags_FaceImages] FOREIGN KEY ([FaceImageId]) 
            REFERENCES [dbo].[FaceImages] ([ID]) ON DELETE CASCADE
    );
    PRINT 'Created FaceTags table';
END
ELSE
BEGIN
    PRINT 'FaceTags table already exists';
END
GO

-- Create indexes for better performance
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_FaceTags_FaceImageId' AND object_id = OBJECT_ID('FaceTags'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_FaceTags_FaceImageId] ON [dbo].[FaceTags] ([FaceImageId]);
    PRINT 'Created index IX_FaceTags_FaceImageId';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_FaceTags_FirstName' AND object_id = OBJECT_ID('FaceTags'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_FaceTags_FirstName] ON [dbo].[FaceTags] ([FirstName]);
    PRINT 'Created index IX_FaceTags_FirstName';
END
GO

PRINT 'Face Recognition migration completed successfully';
