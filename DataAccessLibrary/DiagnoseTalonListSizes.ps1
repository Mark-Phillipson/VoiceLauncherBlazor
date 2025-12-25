# TalonList Column Size Diagnostic Script
# This script helps identify which TalonList values exceed database column size limits

Write-Host "TalonList Column Size Diagnostic Tool" -ForegroundColor Cyan
Write-Host "====================================" -ForegroundColor Cyan
Write-Host ""

# Check if we're in the right directory
$currentDir = Get-Location
$projectPath = "c:\Users\MPhil\source\repos\VoiceLauncherBlazor\DataAccessLibrary"

if ($currentDir.Path -ne $projectPath) {
    Write-Host "Changing to project directory..." -ForegroundColor Yellow
    Set-Location $projectPath
}

Write-Host "Current directory: $(Get-Location)" -ForegroundColor Green
Write-Host ""

# Method 1: Direct SQL Query (if you have access to the database)
Write-Host "METHOD 1: Direct SQL Query" -ForegroundColor Magenta
Write-Host "=========================" -ForegroundColor Magenta
Write-Host "Copy and paste this SQL query into SQL Server Management Studio:" -ForegroundColor Yellow
Write-Host ""

$sqlQuery = @"
-- Find TalonList entries that exceed column size limits
SELECT 
    Id,
    ListName,
    LEN(ListName) as ListNameLength,
    CASE WHEN LEN(ListName) > 100 THEN 'EXCEEDS LIMIT' ELSE 'OK' END as ListNameStatus,
    
    SpokenForm,
    LEN(SpokenForm) as SpokenFormLength, 
    CASE WHEN LEN(SpokenForm) > 100 THEN 'EXCEEDS LIMIT' ELSE 'OK' END as SpokenFormStatus,
    
    LEFT(ListValue, 50) + '...' as ListValueSample,
    LEN(ListValue) as ListValueLength,
    CASE WHEN LEN(ListValue) > 500 THEN 'EXCEEDS LIMIT' ELSE 'OK' END as ListValueStatus,
    
    SourceFile,
    LEN(COALESCE(SourceFile, '')) as SourceFileLength,
    CASE WHEN LEN(COALESCE(SourceFile, '')) > 250 THEN 'EXCEEDS LIMIT' ELSE 'OK' END as SourceFileStatus
    
FROM TalonLists
WHERE LEN(ListName) > 100 
   OR LEN(SpokenForm) > 100
   OR LEN(ListValue) > 500
   OR LEN(COALESCE(SourceFile, '')) > 250
ORDER BY LEN(ListValue) DESC, LEN(ListName) DESC, LEN(SpokenForm) DESC;

-- Summary of all column lengths
SELECT 
    'ListName' as ColumnName,
    100 as CurrentLimit,
    MAX(LEN(ListName)) as ActualMaxLength,
    COUNT(CASE WHEN LEN(ListName) > 100 THEN 1 END) as ViolationCount
FROM TalonLists
UNION ALL
SELECT 
    'SpokenForm' as ColumnName,
    100 as CurrentLimit,
    MAX(LEN(SpokenForm)) as ActualMaxLength,
    COUNT(CASE WHEN LEN(SpokenForm) > 100 THEN 1 END) as ViolationCount
FROM TalonLists  
UNION ALL
SELECT 
    'ListValue' as ColumnName,
    500 as CurrentLimit,
    MAX(LEN(ListValue)) as ActualMaxLength,
    COUNT(CASE WHEN LEN(ListValue) > 500 THEN 1 END) as ViolationCount
FROM TalonLists
UNION ALL
SELECT 
    'SourceFile' as ColumnName,
    250 as CurrentLimit,
    MAX(LEN(COALESCE(SourceFile, ''))) as ActualMaxLength,
    COUNT(CASE WHEN LEN(COALESCE(SourceFile, '')) > 250 THEN 1 END) as ViolationCount
FROM TalonLists;
"@

Write-Host $sqlQuery -ForegroundColor White
Write-Host ""

# Method 2: Entity Framework approach
Write-Host "METHOD 2: Entity Framework Diagnostic (Recommended)" -ForegroundColor Magenta
Write-Host "================================================" -ForegroundColor Magenta
Write-Host "Run this command to build and execute the diagnostic tool:" -ForegroundColor Yellow
Write-Host ""

$efCommand = "dotnet run --configuration Debug"
Write-Host $efCommand -ForegroundColor Green
Write-Host ""

Write-Host "If that doesn't work, try creating a simple test:" -ForegroundColor Yellow

# Method 3: Simple C# test snippet
Write-Host ""
Write-Host "METHOD 3: Simple C# Code Test" -ForegroundColor Magenta  
Write-Host "=============================" -ForegroundColor Magenta
Write-Host "Add this method to your TalonVoiceCommandDataService and call it:" -ForegroundColor Yellow
Write-Host ""

$csharpCode = @"
public async Task<string> QuickDiagnoseTalonListSizes()
{
    var talonLists = await _context.TalonLists.ToListAsync();
    var violations = new List<string>();
    
    foreach (var item in talonLists)
    {
        if (item.ListName?.Length > 100)
            violations.Add($"ListName too long: {item.ListName.Length} chars - '{item.ListName.Substring(0, Math.Min(50, item.ListName.Length))}...'");
        if (item.SpokenForm?.Length > 100) 
            violations.Add($"SpokenForm too long: {item.SpokenForm.Length} chars - '{item.SpokenForm.Substring(0, Math.Min(50, item.SpokenForm.Length))}...'");
        if (item.ListValue?.Length > 500)
            violations.Add($"ListValue too long: {item.ListValue.Length} chars - '{item.ListValue.Substring(0, Math.Min(50, item.ListValue.Length))}...'");
        if (item.SourceFile?.Length > 250)
            violations.Add($"SourceFile too long: {item.SourceFile.Length} chars - '{item.SourceFile}'");
    }
    
    return string.Join(Environment.NewLine, violations);
}
"@

Write-Host $csharpCode -ForegroundColor White
Write-Host ""

# Method 4: PowerShell EF Core command
Write-Host "METHOD 4: Check Current Data with EF Core" -ForegroundColor Magenta
Write-Host "=========================================" -ForegroundColor Magenta
Write-Host "If you have EF Core global tools installed:" -ForegroundColor Yellow
Write-Host ""

Write-Host "dotnet ef database update --configuration Debug" -ForegroundColor Green
Write-Host ""

Write-Host "Then connect to your database and run the SQL from Method 1." -ForegroundColor Yellow
Write-Host ""

# Next steps
Write-Host "NEXT STEPS AFTER IDENTIFYING THE PROBLEM:" -ForegroundColor Red
Write-Host "=========================================" -ForegroundColor Red
Write-Host "1. Run one of the methods above to identify which values are too large" -ForegroundColor Yellow
Write-Host "2. Update the TalonList model with larger StringLength attributes" -ForegroundColor Yellow  
Write-Host "3. Create a new migration:" -ForegroundColor Yellow
Write-Host "   dotnet ef migrations add IncreaseTalonListColumnSizes --configuration Debug" -ForegroundColor Green
Write-Host "4. Generate migration script:" -ForegroundColor Yellow
Write-Host "   dotnet ef migrations script --configuration Debug > TalonListFix.sql" -ForegroundColor Green
Write-Host "5. Apply the script to your database manually" -ForegroundColor Yellow
Write-Host ""

Write-Host "Press any key to continue..." -ForegroundColor Cyan
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
