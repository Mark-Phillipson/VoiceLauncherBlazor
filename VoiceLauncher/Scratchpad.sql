 SELECT TalonVoiceCommands.Application,Mode FROM TalonVoiceCommands GROUP BY Application,mode  ORDER BY Mode desc
  SELECT Repository FROM TalonVoiceCommands GROUP BY Repository
  SELECT DISTINCT ListName,ListValue 
FROM TalonLists 
ORDER BY ListName;

SELECT ListName, ListValue,SpokenForm FROM TalonLists
 WHERE ListName LIKE '%running%'
 GROUP BY ListName,ListValue,SpokenForm ORDER BY ListName,ListValue;

  SELECT Command,Mode,Script,Application,Repository,Title,tags,language,codelanguage FROM TalonVoiceCommands 
  --WHERE  Command LIKE 'talon lists%'  
  ORDER BY codelanguage desc

  SELECT *  FROM Languages 

SELECT
    *
FROM
    dbo.Categories
     WHERE Category='Folders'
     ORDER BY Category