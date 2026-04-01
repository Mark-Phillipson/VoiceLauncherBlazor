# Talon voice commands for launching VoiceLauncherBlazor WinForms app

# command phrase to open search with term
run application voice admin windows forms {user.text}:
    user.run_application_voice_admin_windows_forms({user.text})

# command phrase to open launcher category
run application voice admin windows forms launcher {user.text}:
    user.run_application_voice_admin_windows_forms_launcher({user.text})
