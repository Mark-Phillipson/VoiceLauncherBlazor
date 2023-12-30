using Microsoft.AspNetCore.Components;

namespace RazorClassLibrary.Pages
{
    public partial class Loading
    {
        [Inject] NavigationManager? NavigationManager { get; set; }
        void GoHome()
        {
            NavigationManager!.NavigateTo("/");


        }
    }
}