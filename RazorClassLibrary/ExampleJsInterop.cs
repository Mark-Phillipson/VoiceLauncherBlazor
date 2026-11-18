using Microsoft.JSInterop;

namespace RazorClassLibrary
{
    // This class provides an example of how JavaScript functionality can be wrapped
    // in a .NET class for easy consumption. The associated JavaScript module is
    // loaded on demand when first needed.
    //
    // This class can be registered as scoped DI service and then injected into Blazor
    // components for use.

    public class ExampleJsInterop : IAsyncDisposable
    {
        private readonly Lazy<Task<IJSObjectReference?>> moduleTask;

        public ExampleJsInterop(IJSRuntime jsRuntime)
        {
            moduleTask = new(() => jsRuntime.InvokeAsyncSafe<IJSObjectReference?>(
                "import", "./_content/RazorClassLibrary/exampleJsInterop.js").AsTask());
        }

        public async ValueTask<string> Prompt(string message)
        {
            var module = await moduleTask.Value;
            if (module is null)
            {
                throw new InvalidOperationException("The JavaScript module could not be loaded.");
            }
            return await module.InvokeAsyncSafe<string>("showPrompt", message);
        }

        public async ValueTask DisposeAsync()
        {
            if (moduleTask.IsValueCreated)
            {
                var module = await moduleTask.Value;
                if (module is not null)
                {
                    await module.DisposeAsync();
                }
            }
        }
    }
}