SELECT ID,SendKeys_Value 
FROM CustomIntelliSense
WHERE SendKeys_Value LIKE '%{End}%' 

UPDATE CustomIntelliSense
SET SendKeys_Value = REPLACE(SendKeys_Value, '{End}', '')
WHERE SendKeys_Value LIKE '%{End}%';

--Enter Right 3 End