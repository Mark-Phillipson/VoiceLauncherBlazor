namespace TalonVoiceCommands.Client.Pages
{
    public partial class Home
    {
        private string? WelcomeMessage { get; set; }

        protected override void OnInitialized()
        {
            WelcomeMessage = "Welcome to the Talon Voice Commands!";
        }
    }
}