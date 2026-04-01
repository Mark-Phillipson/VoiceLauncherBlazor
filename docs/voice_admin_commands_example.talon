# Talon voice commands for launching VoiceLauncherBlazor WinForms app
# Snippet Category List(https://github.com/Mark-Phillipson/talon_my_stuff/blob/c087afce166b942be216cdbd01aab98669ae67ab/custom_voice_coding/custom_snippet_category.py#L1-L79)
# Launcher Category List(https://github.com/Mark-Phillipson/talon_my_stuff/blob/c087afce166b942be216cdbd01aab98669ae67ab/custom_voice_coding/custom_snippet_languages.py#L1-L28)
# Exact command mappings requested by user
search list {user.text}:
    user.run_application_voice_admin_windows_forms({user.text})

{user.snippet_language} {user.snippet_category}:
    user.run_application_voice_admin_windows_forms_language_category({user.snippet_language}, {user.snippet_category})

launch {user.launcher_category}:
    user.run_application_voice_admin_windows_forms_launcher({user.launcher_category})

what can I say:
    user.run_application_voice_admin_windows_forms_launcher("Talon Search")

what can I say new:
    user.launch_talon_voice_command_server()

launch {user.launcher_category} {user.text}:
    user.run_application_voice_admin_windows_forms_launcher_with_parameter({user.launcher_category}, {user.text})
