
SELECT *
FROM Categories
WHERE Colour = '#3357ff'

UPDATE Categories
SET Colour = (
    SELECT Colour
FROM (
                                                                                SELECT '#00ff99' AS Colour
    UNION ALL
        SELECT '#33ff09' AS Colour
    UNION ALL
        SELECT '#3907ff' AS Colour
    UNION ALL
        SELECT '#ff33a1' AS Colour
    UNION ALL
        SELECT '#0405ff' AS Colour
    UNION ALL
        SELECT '#a133ff' AS Colour
    ) AS RandomColors
ORDER BY NEWID()
    OFFSET 0 ROWS FETCH NEXT 1 ROW ONLY
)
WHERE Colour = '#3357ff';