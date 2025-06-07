# PowerShell script to run Playwright MSTest tests in headed mode

$env:PLAYWRIGHT_HEADLESS = "false"
dotnet test /UpworkSearching.csproj
