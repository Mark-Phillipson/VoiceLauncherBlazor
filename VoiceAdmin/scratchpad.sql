 SELECT SendKeys_Value FROM CustomIntelliSense
 WHERE SendKeys_Value LIKE '%+%';

 BEGIN;

-- Exact Dragon sequence removal
UPDATE CustomIntelliSense
SET SendKeys_Value = REPLACE(SendKeys_Value, '+', '')
WHERE SendKeys_Value LIKE '%+';

-- Remove common standalone snippets
UPDATE CustomIntelliSense
SET SendKeys_Value = REPLACE(SendKeys_Value, '{Enter}', '')
WHERE SendKeys_Value LIKE '%{Enter}%';

UPDATE CustomIntelliSense
SET SendKeys_Value = REPLACE(SendKeys_Value, '{End}', '')
WHERE SendKeys_Value LIKE '%{End}%';

UPDATE CustomIntelliSense
SET SendKeys_Value = REPLACE(SendKeys_Value, '{Left}', '')
WHERE SendKeys_Value LIKE '%{Left}%';

SELECT SendKeys_Value FROM CustomIntelliSense
WHERE SendKeys_Value LIKE '%{Left}%';

UPDATE CustomIntelliSense
SET SendKeys_Value = REPLACE(SendKeys_Value, '{Right}', '')
WHERE SendKeys_Value LIKE '%{Right}%';

UPDATE CustomIntelliSense
SET SendKeys_Value = REPLACE(SendKeys_Value, '{Up}', '')
WHERE SendKeys_Value LIKE '%{Up}%';

UPDATE CustomIntelliSense
SET SendKeys_Value = REPLACE(SendKeys_Value, '{Down}', '')
WHERE SendKeys_Value LIKE '%{Down}%';

-- Common numeric cursor moves (example range 1..20)
UPDATE CustomIntelliSense
SET SendKeys_Value = REPLACE(SendKeys_Value, '{Left 1}', '')
WHERE SendKeys_Value LIKE '%{Left 1}%';

UPDATE CustomIntelliSense
SET SendKeys_Value = REPLACE(SendKeys_Value, '{Left 2}', '')
WHERE SendKeys_Value LIKE '%{Left 2}%';

UPDATE CustomIntelliSense
SET SendKeys_Value = REPLACE(SendKeys_Value, '{Left 3}', '')
WHERE SendKeys_Value LIKE '%{Left 3}%';

UPDATE CustomIntelliSense
SET SendKeys_Value = REPLACE(SendKeys_Value, '{Left 4}', '')
WHERE SendKeys_Value LIKE '%{Left 4}%';

UPDATE CustomIntelliSense
SET SendKeys_Value = REPLACE(SendKeys_Value, '{Left 5}', '')
WHERE SendKeys_Value LIKE '%{Left 5}%';

UPDATE CustomIntelliSense
SET SendKeys_Value = REPLACE(SendKeys_Value, '{Left 6}', '')
WHERE SendKeys_Value LIKE '%{Left 6}%';

UPDATE CustomIntelliSense
SET SendKeys_Value = REPLACE(SendKeys_Value, '{Left 7}', '')
WHERE SendKeys_Value LIKE '%{Left 7}%';

UPDATE CustomIntelliSense
SET SendKeys_Value = REPLACE(SendKeys_Value, '{Left 8}', '')
WHERE SendKeys_Value LIKE '%{Left 8}%';

UPDATE CustomIntelliSense
SET SendKeys_Value = REPLACE(SendKeys_Value, '{Left 9}', '')
WHERE SendKeys_Value LIKE '%{Left 9}%';

UPDATE CustomIntelliSense
SET SendKeys_Value = REPLACE(SendKeys_Value, '{Left 10}', '')
WHERE SendKeys_Value LIKE '%{Left 10}%';

UPDATE CustomIntelliSense
SET SendKeys_Value = REPLACE(SendKeys_Value, '{Left 11}', '')
WHERE SendKeys_Value LIKE '%{Left 11}%';

UPDATE CustomIntelliSense
SET SendKeys_Value = REPLACE(SendKeys_Value, '{Left 12}', '')
WHERE SendKeys_Value LIKE '%{Left 12}%';

UPDATE CustomIntelliSense
SET SendKeys_Value = REPLACE(SendKeys_Value, '{Left 13}', '')
WHERE SendKeys_Value LIKE '%{Left 13}%';

UPDATE CustomIntelliSense
SET SendKeys_Value = REPLACE(SendKeys_Value, '{Left 14}', '')
WHERE SendKeys_Value LIKE '%{Left 14}%';

UPDATE CustomIntelliSense
SET SendKeys_Value = REPLACE(SendKeys_Value, '{Left 15}', '')
WHERE SendKeys_Value LIKE '%{Left 15}%';

UPDATE CustomIntelliSense
SET SendKeys_Value = REPLACE(SendKeys_Value, '{Left 16}', '')
WHERE SendKeys_Value LIKE '%{Left 16}%';

UPDATE CustomIntelliSense
SET SendKeys_Value = REPLACE(SendKeys_Value, '{Left 17}', '')
WHERE SendKeys_Value LIKE '%{Left 17}%';

UPDATE CustomIntelliSense
SET SendKeys_Value = REPLACE(SendKeys_Value, '{Left 18}', '')
WHERE SendKeys_Value LIKE '%{Left 18}%';

UPDATE CustomIntelliSense
SET SendKeys_Value = REPLACE(SendKeys_Value, '{Left 19}', '')
WHERE SendKeys_Value LIKE '%{Left 19}%';

UPDATE CustomIntelliSense
SET SendKeys_Value = REPLACE(SendKeys_Value, '{Left 20}', '')
WHERE SendKeys_Value LIKE '%{Left 20}%';

-- Similar for Right...
UPDATE CustomIntelliSense
SET SendKeys_Value = REPLACE(SendKeys_Value, '{Right 1}', '')
WHERE SendKeys_Value LIKE '%{Right 1}%';

UPDATE CustomIntelliSense
SET SendKeys_Value = REPLACE(SendKeys_Value, '{Right 2}', '')
WHERE SendKeys_Value LIKE '%{Right 2}%';

UPDATE CustomIntelliSense
SET SendKeys_Value = REPLACE(SendKeys_Value, '{Right 3}', '')
WHERE SendKeys_Value LIKE '%{Right 3}%';

UPDATE CustomIntelliSense
SET SendKeys_Value = REPLACE(SendKeys_Value, '{Right 4}', '')
WHERE SendKeys_Value LIKE '%{Right 4}%';

UPDATE CustomIntelliSense
SET SendKeys_Value = REPLACE(SendKeys_Value, '{Right 5}', '')
WHERE SendKeys_Value LIKE '%{Right 5}%';

UPDATE CustomIntelliSense
SET SendKeys_Value = REPLACE(SendKeys_Value, '{Right 6}', '')
WHERE SendKeys_Value LIKE '%{Right 6}%';

UPDATE CustomIntelliSense
SET SendKeys_Value = REPLACE(SendKeys_Value, '{Right 7}', '')
WHERE SendKeys_Value LIKE '%{Right 7}%';

UPDATE CustomIntelliSense
SET SendKeys_Value = REPLACE(SendKeys_Value, '{Right 8}', '')
WHERE SendKeys_Value LIKE '%{Right 8}%';

UPDATE CustomIntelliSense
SET SendKeys_Value = REPLACE(SendKeys_Value, '{Right 9}', '')
WHERE SendKeys_Value LIKE '%{Right 9}%';

UPDATE CustomIntelliSense
SET SendKeys_Value = REPLACE(SendKeys_Value, '{Right 10}', '')
WHERE SendKeys_Value LIKE '%{Right 10}%';

UPDATE CustomIntelliSense
SET SendKeys_Value = REPLACE(SendKeys_Value, '{Right 11}', '')
WHERE SendKeys_Value LIKE '%{Right 11}%';

UPDATE CustomIntelliSense
SET SendKeys_Value = REPLACE(SendKeys_Value, '{Right 12}', '')
WHERE SendKeys_Value LIKE '%{Right 12}%';

UPDATE CustomIntelliSense
SET SendKeys_Value = REPLACE(SendKeys_Value, '{Right 13}', '')
WHERE SendKeys_Value LIKE '%{Right 13}%';

UPDATE CustomIntelliSense
SET SendKeys_Value = REPLACE(SendKeys_Value, '{Right 14}', '')
WHERE SendKeys_Value LIKE '%{Right 14}%';

UPDATE CustomIntelliSense
SET SendKeys_Value = REPLACE(SendKeys_Value, '{Right 15}', '')
WHERE SendKeys_Value LIKE '%{Right 15}%';

UPDATE CustomIntelliSense
SET SendKeys_Value = REPLACE(SendKeys_Value, '{Right 16}', '')
WHERE SendKeys_Value LIKE '%{Right 16}%';

UPDATE CustomIntelliSense
SET SendKeys_Value = REPLACE(SendKeys_Value, '{Right 17}', '')
WHERE SendKeys_Value LIKE '%{Right 17}%';

UPDATE CustomIntelliSense
SET SendKeys_Value = REPLACE(SendKeys_Value, '{Right 18}', '')
WHERE SendKeys_Value LIKE '%{Right 18}%';

UPDATE CustomIntelliSense
SET SendKeys_Value = REPLACE(SendKeys_Value, '{Right 19}', '')
WHERE SendKeys_Value LIKE '%{Right 19}%';

UPDATE CustomIntelliSense
SET SendKeys_Value = REPLACE(SendKeys_Value, '{Right 20}', '')
WHERE SendKeys_Value LIKE '%{Right 20}%';

-- cleanup spaces
UPDATE CustomIntelliSense
SET SendKeys_Value = TRIM(REPLACE(REPLACE(SendKeys_Value, '  ', ' '), '  ', ' '))
WHERE SendKeys_Value LIKE '%  %';

COMMIT
end;