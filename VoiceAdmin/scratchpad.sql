 SELECT Colour,Count(*) As ColourCount FROM categories  GROUP BY  Colour ORDER BY ColourCount DESC;  
   SELECT * FROM categories WHERE Colour = '#0405ff';

   UPDATE categories
SET Colour = '#' + SUBSTRING(REPLACE(NEWID(), '-', ''), 1, 6)
WHERE Colour = '#0405ff';