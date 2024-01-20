using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace RazorClassLibrary.Pages
{
    public partial class GlobalSearch : ComponentBase
    {
    [Inject] public required IJSRuntime JSRuntime { get; set; }
         private  string searchTerm { get; set; } = "";
         protected override async Task OnAfterRenderAsync(bool firstRender)
                 {
                     if (firstRender)
                     {
                         try
                         {
                             if (JSRuntime != null)
                             {
                                 await JSRuntime.InvokeVoidAsync("window.setFocus", "searchTerm");
                             }
                         }
                         catch (Exception exception)
                         {
                             Console.WriteLine(exception.Message);
                         }
                     }
                 }
    }
}