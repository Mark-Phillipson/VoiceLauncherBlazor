-- Debug query to check TalonLists for "four o"
SELECT 
    ListName, 
    SpokenForm, 
    ListValue,
    'Match in SpokenForm' as MatchType
FROM TalonLists 
WHERE SpokenForm LIKE '%four o%'

UNION ALL

SELECT 
    ListName, 
    SpokenForm, 
    ListValue,
    'Match in ListValue' as MatchType
FROM TalonLists 
WHERE ListValue LIKE '%four o%'

ORDER BY ListName, MatchType;

-- Check for commands that reference the model list
SELECT
    Command,
    SUBSTR(Script, 1, 100) as ScriptPreview,
    Application,
    Mode
FROM TalonVoiceCommands
WHERE Script LIKE '%{model}%' 
   OR Script LIKE '%{user.model}%'
ORDER BY Command
LIMIT 10;
