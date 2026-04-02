---
title: GitHub Copilot CLI - Getting Started
---

# GitHub Copilot CLI - Getting Started

This guide will help you set up and use the GitHub Copilot CLI on Windows with PowerShell, including speech-friendly aliases and example commands for best results with voice recognition.


## 1. Using Copilot CLI with Talon Voice Commands

You do not need to set up PowerShell aliases. Instead, use the Copilot CLI commands directly, or use Talon voice commands to insert them for you.


## 2. Example Commands

Use these commands directly in your terminal:

### Get a shell command suggestion
```powershell
github-copilot-cli what-the-shell "list all files in the current directory"
```

### Get a git command suggestion
```powershell
github-copilot-cli git-assist "undo the last commit but keep the changes"
```

### Get a GitHub CLI command suggestion
```powershell
github-copilot-cli gh-assist "create a new issue in this repo"
```

### Get help for a subcommand
```powershell
github-copilot-cli help what-the-shell
```


## 3. Troubleshooting

- If you get a 'not recognized' error, check that `github-copilot-cli` is installed and in your PATH.
- You can always use the full command `github-copilot-cli` if Talon or your terminal cannot find it.

---
Happy coding with Copilot CLI!

---

## 4. Talon Voice Commands for Copilot CLI

If you use Talon for voice control, here are some example voice commands you can use to trigger Copilot CLI actions:

| Voice Command     | Action inserted in terminal                        |
|-------------------|---------------------------------------------------|
| shelling          | github-copilot-cli what-the-shell                 |
| git assist        | github-copilot-cli git-assist                     |
| github assist     | github-copilot-cli gh-assist                      |

You can say these commands to quickly insert the corresponding Copilot CLI command in your terminal.