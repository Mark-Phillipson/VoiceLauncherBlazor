-- Migration Script: Add ImageLink column to Language table
ALTER TABLE [dbo].[Languages]
ADD [ImageLink] NVARCHAR(200) NULL;
