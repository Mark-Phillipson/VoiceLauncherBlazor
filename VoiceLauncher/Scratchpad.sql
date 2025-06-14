 SELECT TalonVoiceCommands.Application,Mode FROM TalonVoiceCommands GROUP BY Application,mode  ORDER BY Mode desc
  SELECT Repository FROM TalonVoiceCommands GROUP BY Repository
  SELECT DISTINCT ListName,ListValue 
FROM TalonLists 
ORDER BY ListName;

SELECT ListName, COUNT(*) as ItemCount FROM TalonLists GROUP BY ListName ORDER BY ListName;