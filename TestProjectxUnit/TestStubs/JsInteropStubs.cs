using Bunit;

namespace TestProjectxUnit.TestStubs
{
    public static class JsInteropStubs
    {
        /// <summary>
        /// Configure JSInterop stubs used by TalonVoiceCommandSearch component tests.
        /// - Stubs the module import for selectionModalInterop.js and its showModal method.
        /// - Stubs the global bootstrapInterop.showModal fallback.
        /// </summary>
        public static void ConfigureSelectionModalInterop(TestContext ctx)
        {
            if (ctx == null) return;

            try
            {
                // Stub the ES module import that the component calls
                var module = ctx.JSInterop.SetupModule("/_content/RazorClassLibrary/selectionModalInterop.js");
                module.SetupVoid("showModal", args => true);

                // Stub the global fallback used in ShowSelectionModalAsync
                ctx.JSInterop.SetupVoid("bootstrapInterop.showModal", args => true);
                // Stub common JS functions used in the component to avoid bUnit diagnostics
                ctx.JSInterop.SetupVoid("console.log", _ => true);
                ctx.JSInterop.SetupVoid("console.debug", _ => true);
                ctx.JSInterop.SetupVoid("console.error", _ => true);
                ctx.JSInterop.SetupVoid("setTimeout", args => true);
                ctx.JSInterop.SetupVoid("window.open", args => true);
            }
            catch
            {
                // Swallow any setup errors during test discovery; tests will fail later if JSInterop isn't available
            }
        }
    }
}
