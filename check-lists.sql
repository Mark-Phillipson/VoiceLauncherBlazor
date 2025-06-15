-- Check what lists exist in the database
SELECT DISTINCT ListName, COUNT(*) as ItemCount 
FROM TalonLists 
GROUP BY ListName 
ORDER BY ListName;

-- Check for any lists that contain "model" in the name
SELECT DISTINCT ListName, COUNT(*) as ItemCount 
FROM TalonLists 
WHERE ListName LIKE '%model%'
GROUP BY ListName 
ORDER BY ListName;

-- Show a sample of items from user.model if it exists
SELECT TOP 10 ListName, SpokenForm, ListValue 
FROM TalonLists 
WHERE ListName = 'user.model' 
ORDER BY SpokenForm;
