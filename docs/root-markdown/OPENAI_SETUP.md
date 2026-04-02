# OpenAI API Key Setup for AI Chat Component

## Overview
The AI Chat component in the Windows Hybrid application requires an OpenAI API key to function. This guide explains how to set up the API key securely.

## Configuration Options

The application will look for your OpenAI API key in the following order:

1. **User Secrets** (Recommended for development)
2. **Environment Variable**
3. **appsettings.json** (Not recommended for production)

## Setup Methods

### Method 1: User Secrets (Recommended)

User secrets provide a secure way to store sensitive configuration data during development.

1. Open a terminal in the WinFormsApp directory
2. Set your OpenAI API key using one of these commands:

```powershell
# Primary configuration path
dotnet user-secrets set "SmartComponents:ApiKey" "your-actual-openai-api-key-here"

# Alternative configuration path
dotnet user-secrets set "OpenAI:ApiKey" "your-actual-openai-api-key-here"
```

### Method 2: Environment Variable

Set the `OPENAI_API_KEY` environment variable:

**Windows (PowerShell):**
```powershell
$env:OPENAI_API_KEY = "your-actual-openai-api-key-here"
```

**Windows (Command Prompt):**
```cmd
set OPENAI_API_KEY=your-actual-openai-api-key-here
```

**To make it permanent (Windows):**
1. Open System Properties → Advanced → Environment Variables
2. Add a new user variable:
   - Name: `OPENAI_API_KEY`
   - Value: `your-actual-openai-api-key-here`

### Method 3: appsettings.json (Development Only)

⚠️ **Warning**: Never commit API keys to source control!

Edit `WinFormsApp/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "..."
  },
  "OpenAI": {
    "ApiKey": "your-actual-openai-api-key-here"
  },
  "SmartComponents": {
    "ApiKey": "your-actual-openai-api-key-here"
  }
}
```

## Getting an OpenAI API Key

1. Visit [OpenAI's platform](https://platform.openai.com/)
2. Sign up or log in to your account
3. Navigate to API Keys section
4. Create a new API key
5. Copy the key and use it in one of the configuration methods above

## Verification

After setting up your API key:

1. Build the application: `dotnet build`
2. Run the application: `dotnet run`
3. Click the "AI Chat" button
4. If configured correctly, you should see no error messages
5. If there's an issue, check the error message for guidance

## Troubleshooting

**"OpenAI API key not found" error:**
- Verify the API key is set correctly using one of the methods above
- Restart the application after setting environment variables
- Check for typos in the configuration key names

**Build errors with locked DLL files:**
- Stop any running instances of the application
- Close Visual Studio if open
- Clean and rebuild: `dotnet clean && dotnet build`

## Security Best Practices

1. **Never commit API keys to source control**
2. Use User Secrets for development
3. Use secure key management services for production
4. Rotate API keys regularly
5. Monitor API usage and set usage limits

## Configuration Paths Reference

The application checks these configuration paths in order:

1. `SmartComponents:ApiKey`
2. `OpenAI:ApiKey`  
3. Environment variable: `OPENAI_API_KEY`

Use any one of these - the application will find and use the first available key.
