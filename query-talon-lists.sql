-- SQL Server commands to query TalonLists table

-- 1. View all unique list names
SELECT DISTINCT ListName 
FROM TalonLists 
ORDER BY ListName;

-- 2. Count items per list
SELECT ListName, COUNT(*) as ItemCount
FROM TalonLists 
GROUP BY ListName 
ORDER BY ListName;

-- 3. View all lists with their spoken forms and values (first 50 items)
SELECT ListName, SpokenForm, ListValue
FROM TalonLists 
ORDER BY ListName, SpokenForm
LIMIT 50;

-- 4. Search for specific list (example: git-related lists)
SELECT DISTINCT ListName 
FROM TalonLists 
WHERE ListName LIKE '%git%' 
ORDER BY ListName;

-- 5. View a specific list's contents (example: user.git_argument)
SELECT SpokenForm, ListValue
FROM TalonLists 
WHERE ListName = 'user.git_argument'
ORDER BY SpokenForm;

-- 6. Count total lists and total items
SELECT 
    COUNT(DISTINCT ListName) as TotalLists,
    COUNT(*) as TotalItems
FROM TalonLists;

-- 7. Find lists that contain specific spoken forms or values
SELECT DISTINCT ListName
FROM TalonLists 
WHERE SpokenForm LIKE '%abort%' OR ListValue LIKE '%abort%'
ORDER BY ListName;

-- 8. View sample of each list (first 3 items per list)
WITH RankedLists AS (
    SELECT ListName, SpokenForm, ListValue,
           ROW_NUMBER() OVER (PARTITION BY ListName ORDER BY SpokenForm) as rn
    FROM TalonLists
)
SELECT ListName, SpokenForm, ListValue
FROM RankedLists 
WHERE rn <= 3
ORDER BY ListName, SpokenForm;
