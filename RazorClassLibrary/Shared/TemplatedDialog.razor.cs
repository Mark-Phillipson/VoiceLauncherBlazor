using Microsoft.AspNetCore.Components;

namespace RazorClassLibrary.Shared
{
    public partial class TemplatedDialog : ComponentBase
    {
        [Parameter] public RenderFragment? ChildContent { get; set; }
        [Parameter] public bool Show { get; set; }
        [Parameter] public int PageYOffset { get; set; }
        private string top { get; set; } = "0px";
        protected override void OnParametersSet()
        {
            top = $"{PageYOffset}px";
        }
    }
}