using Microsoft.AspNetCore.Components;

namespace RazorClassLibrary.Pages
{
    public partial class Loading
    {
        [Inject] public required NavigationManager NavigationManager { get; set; }
        void GoHome()
        {
            NavigationManager!.NavigateTo("/");


        }
    }
}