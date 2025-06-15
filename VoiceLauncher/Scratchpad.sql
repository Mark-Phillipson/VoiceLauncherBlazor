 SELECT TalonVoiceCommands.Application,Mode FROM TalonVoiceCommands GROUP BY Application,mode  ORDER BY Mode desc
  SELECT Repository FROM TalonVoiceCommands GROUP BY Repository
  SELECT DISTINCT ListName,ListValue 
FROM TalonLists 
ORDER BY ListName;

SELECT ListName, ListValue FROM TalonLists
 WHERE ListName='user.model'
 GROUP BY ListName,ListValue ORDER BY ListName,ListValue;

  SELECT Command,Mode,Script,Application,Repository FROM TalonVoiceCommands WHERE  Command LIKE 'talon lists%'  ORDER BY Command