-- Step 1: Check if the table names are correct 
-- (The error might be due to "Categories" vs "Category" table names)
PRINT 'Checking table names...';
SELECT TABLE_NAME 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_TYPE = 'BASE TABLE' AND 
      (TABLE_NAME LIKE '%Category%' OR TABLE_NAME LIKE '%Categories%' OR TABLE_NAME LIKE '%Launcher%');

-- Step 2: Verify foreign key constraint names
PRINT 'Checking foreign key constraints...';
SELECT name AS ForeignKeyName, 
       OBJECT_NAME(parent_object_id) AS TableName,
       OBJECT_NAME(referenced_object_id) AS ReferencedTableName
FROM sys.foreign_keys
WHERE OBJECT_NAME(parent_object_id) = 'LauncherCategoryBridge' OR 
      OBJECT_NAME(referenced_object_id) = 'LauncherCategoryBridge';

-- Step 3: Verify the correct column names in the Category/Categories table
PRINT 'Checking Category table columns...';
SELECT COLUMN_NAME, DATA_TYPE 
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME LIKE '%Categor%';

-- Step 4: Verify the correct column names in the Launcher table
PRINT 'Checking Launcher table columns...';
SELECT COLUMN_NAME, DATA_TYPE 
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Launcher';

-- Step 5: Check if LauncherCategoryBridge table exists
PRINT 'Checking if LauncherCategoryBridge table exists...';
IF OBJECT_ID('LauncherCategoryBridge', 'U') IS NOT NULL
    PRINT 'LauncherCategoryBridge table exists.'
ELSE
    PRINT 'LauncherCategoryBridge table does not exist.';

-- Step 6: Fix the migration by recreating the table with proper references
PRINT 'Creating or updating the LauncherCategoryBridge table...';
BEGIN TRY
    -- Drop the table if it exists but has incorrect references
    IF OBJECT_ID('LauncherCategoryBridge', 'U') IS NOT NULL
    BEGIN
        PRINT 'Dropping existing LauncherCategoryBridge table...';
        DROP TABLE LauncherCategoryBridge;
    END
    
    -- Create the table with the proper references
    -- (Adjust table and column names based on what was found in steps 1-4)
    PRINT 'Creating LauncherCategoryBridge table...';
    CREATE TABLE LauncherCategoryBridge (
        ID INT IDENTITY(1,1) PRIMARY KEY,
        LauncherID INT NOT NULL,
        CategoryID INT NOT NULL,
        -- Use the actual table name found in step 1
        CONSTRAINT FK_LauncherCategoryBridge_Launcher FOREIGN KEY (LauncherID) 
            REFERENCES Launcher(ID),
        -- Use either Categories or Category depending on what was found in step 1
        CONSTRAINT FK_LauncherCategoryBridge_Category FOREIGN KEY (CategoryID) 
            REFERENCES Categories(ID)
    );
    
    PRINT 'Creating indexes...';
    CREATE INDEX IX_LauncherCategoryBridge_CategoryID ON LauncherCategoryBridge (CategoryID);
    CREATE INDEX IX_LauncherCategoryBridge_LauncherID ON LauncherCategoryBridge (LauncherID);
    
    -- Insert existing relationships into the bridge table
    PRINT 'Migrating existing launcher-category relationships...';
    INSERT INTO LauncherCategoryBridge (LauncherID, CategoryID)
    SELECT ID, CategoryID 
    FROM Launcher 
    WHERE CategoryID IS NOT NULL;
    
    PRINT 'Migration complete!';
END TRY
BEGIN CATCH
    PRINT 'Error occurred during migration: ' + ERROR_MESSAGE();
END CATCH

-- Step 7: Verify migration results
PRINT 'Verifying migrated data...';
SELECT 
    L.ID AS LauncherID,
    L.Name AS LauncherName,
    L.CategoryID AS OriginalCategoryID,
    B.CategoryID AS BridgeCategoryID,
    B.ID AS BridgeID
FROM Launcher L
LEFT JOIN LauncherCategoryBridge B ON L.ID = B.LauncherID
WHERE L.CategoryID IS NOT NULL
ORDER BY L.ID;
