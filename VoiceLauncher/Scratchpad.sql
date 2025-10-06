 SELECT Repository FROM TalonVoiceCommands GROUP BY Repository;
  SELECT OperatingSystem FROM TalonVoiceCommands GROUP BY OperatingSystem;
   SELECT Application, COUNT(*) as ApplicationCount FROM TalonVoiceCommands GROUP BY Application ORDER BY 
   ApplicationCount DESC;
    SELECT Mode FROM TalonVoiceCommands GROUP BY Mode;