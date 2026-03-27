C:\Users\MPhil\source\repos\VoiceLauncherBlazor\VoiceAdmin\bin\Release\net10.0\win-x64\publish

# First, build the project
dotnet build VoiceAdmin/VoiceAdmin.csproj --configuration Debug

# Publish to a local folder
dotnet publish VoiceAdmin/VoiceAdmin.csproj --configuration Debug --output ./publish

# Deploy to Azure using ZIP deploy
az login
az webapp deploy --resource-group functions-msp --name voicelauncherblazor --src-path ./publish.zip --type zip