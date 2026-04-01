from talon import Module, ui

mod = Module()

@mod.action_class
class Actions:
    def run_application_voice_admin_windows_forms(self, searchTerm: str):
        """Runs VoiceAdmin WinForms app with the given search term"""
        commandline = r"C:\Users\<you>\Downloads\VoiceLauncherBlazor-win-x64\WinFormsApp\WinFormsApp.exe"
        args = [' /SearchIntelliSense', f'/{searchTerm}']
        cwd = r"C:\Users\<you>\Downloads\VoiceLauncherBlazor-win-x64\WinFormsApp"
        ui.launch(path=commandline, args=args, cwd=cwd)

    def run_application_voice_admin_windows_forms_launcher(self, category: str):
        """Launches VoiceAdmin WinForms app with a launcher category"""
        commandline = r"C:\Users\<you>\Downloads\VoiceLauncherBlazor-win-x64\WinFormsApp\WinFormsApp.exe"
        args = [' /Launcher', f'/{category}']
        cwd = r"C:\Users\<you>\Downloads\VoiceLauncherBlazor-win-x64\WinFormsApp"
        ui.launch(path=commandline, args=args, cwd=cwd)
