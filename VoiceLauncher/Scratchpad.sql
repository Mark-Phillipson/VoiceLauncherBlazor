 SELECT * FROM Launcher WHERE Icon LIKE '%/%'
 UPDATE Launcher
SET Icon = REPLACE(Icon, 's/', '')
WHERE Icon LIKE '%s/%';