I'm getting the following in the developer tools console:

assetsCache.ts:30 dotnet Loaded 6.38 MB resourcesThis application was built with linking (tree shaking) disabled. Published applications will be significantly smaller if you install wasm-tools workload. See also https://aka.ms/dotnet-wasm-features
assetsCache.ts:43 Loaded 6.38 MB resources from network
blazor.webassembly.js:1 Debugging hotkey: Shift+Alt+D (when application has focus)
blazor.webassembly.js:1  crit: Microsoft.AspNetCore.Components.WebAssembly.Rendering.WebAssemblyRenderer[100]
      Unhandled exception rendering component: TypeError: Cannot read properties of undefined (reading 'getI32')
TypeError: Cannot read properties of undefined (reading 'getI32')
Dt @ blazor.webassembly.js:1
blazor.webassembly.js:1  crit: Microsoft.AspNetCore.Components.WebAssembly.Rendering.WebAssemblyRenderer[100]
      Unhandled exception rendering component: TypeError: Cannot read properties of undefined (reading 'getI32')
TypeError: Cannot read properties of undefined (reading 'getI32')
Dt @ blazor.webassembly.js:1
