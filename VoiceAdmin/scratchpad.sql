 SELECT Colour,Count(*) As ColourCount FROM categories  GROUP BY  Colour ORDER BY ColourCount DESC;  
   SELECT * FROM categories WHERE Colour = '#0405ff';

   UPDATE categories
-- Note: SQL Server NEWID() used to generate hex color; consider using application-side Guid.NewGuid() instead.
SET Colour = '#' || SUBSTR(REPLACE(LOWER(HEX(randomblob(16))), '-', ''), 1, 6)
WHERE Colour = '#0405ff';