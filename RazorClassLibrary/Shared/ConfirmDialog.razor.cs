using Microsoft.AspNetCore.Components;

namespace RazorClassLibrary.Shared
{
    public partial class ConfirmDialog : ComponentBase
    {
        [Parameter] public EventCallback OnCancel { get; set; }
        [Parameter] public EventCallback OnConfirm { get; set; }
        [Parameter] public string? Title { get; set; }
        [Parameter] public string? Message { get; set; }
        [Parameter] public string ButtonColour { get; set; } = "danger";
    }
}