-- Step 1: Create the bridge table if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'LauncherCategoryBridge')
BEGIN
    CREATE TABLE LauncherCategoryBridge (
        ID INT IDENTITY(1,1) PRIMARY KEY,
        LauncherID INT NOT NULL,
        CategoryID INT NOT NULL,
        CONSTRAINT FK_LauncherCategoryBridge_Launcher FOREIGN KEY (LauncherID) REFERENCES Launcher(ID),
        CONSTRAINT FK_LauncherCategoryBridge_Category FOREIGN KEY (CategoryID) REFERENCES Category(ID)
    );
    
    -- Create a unique constraint to prevent duplicate relationships
    CREATE UNIQUE INDEX IX_LauncherCategoryBridge_Unique ON LauncherCategoryBridge (LauncherID, CategoryID);
END

-- Step 2: Insert existing relationships into the bridge table
INSERT INTO LauncherCategoryBridge (LauncherID, CategoryID)
SELECT ID, CategoryID 
FROM Launcher 
WHERE CategoryID IS NOT NULL
  -- Only insert relationships that don't already exist in the bridge table
  AND NOT EXISTS (
    SELECT 1 FROM LauncherCategoryBridge 
    WHERE LauncherID = Launcher.ID AND CategoryID = Launcher.CategoryID
  );

-- Step 3: Generate a results set showing all the migrations performed
SELECT 
    L.ID AS LauncherID,
    L.Name AS LauncherName,
    L.CategoryID AS OldCategoryID,
    C.CategoryName,
    B.ID AS BridgeID
FROM Launcher L
LEFT JOIN LauncherCategoryBridge B ON L.ID = B.LauncherID AND L.CategoryID = B.CategoryID
LEFT JOIN Category C ON L.CategoryID = C.ID
WHERE L.CategoryID IS NOT NULL
ORDER BY L.ID;

-- Step 4: Create script for later removal of CategoryID column (commented out for safety)
-- After confirming the migration works, you can run this:
/*
ALTER TABLE Launcher DROP CONSTRAINT FK_Launcher_Category; -- Replace with your actual FK constraint name
ALTER TABLE Launcher DROP COLUMN CategoryID;
*/
 SELECT * FROM LauncherCategoryBridge;