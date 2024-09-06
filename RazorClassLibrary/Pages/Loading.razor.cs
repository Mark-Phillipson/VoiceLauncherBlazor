using Microsoft.AspNetCore.Components;

namespace RazorClassLibrary.Pages
{
    public partial class Loading : ComponentBase
    {
        [Inject] public required NavigationManager NavigationManager { get; set; }
        void GoHome()
        {
            NavigationManager!.NavigateTo("/");


        }
    }
}