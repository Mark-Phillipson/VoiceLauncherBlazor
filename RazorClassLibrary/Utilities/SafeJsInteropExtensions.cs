using System.Diagnostics;
using Microsoft.JSInterop;

namespace RazorClassLibrary.Utilities;

/// <summary>
/// Runs optional browser interop without allowing a disconnected or unavailable
/// JavaScript runtime to break the surrounding component operation.
/// </summary>
public static class SafeJsInteropExtensions
{
#pragma warning disable BL0016 // The calls below are deliberately guarded in one shared location.
    public static async ValueTask<bool> InvokeVoidAsyncSafe(
        this IJSRuntime jsRuntime,
        string identifier,
        params object?[]? args)
    {
        try
        {
            await jsRuntime.InvokeVoidAsync(identifier, args);
            return true;
        }
        catch (JSException exception)
        {
            Debug.WriteLine($"JavaScript interop '{identifier}' failed: {exception.Message}");
            return false;
        }
        catch (InvalidOperationException exception)
        {
            Debug.WriteLine($"JavaScript interop '{identifier}' is unavailable: {exception.Message}");
            return false;
        }
    }

    public static async ValueTask<T> InvokeAsyncSafe<T>(
        this IJSRuntime jsRuntime,
        string identifier,
        params object?[]? args)
    {
        try
        {
            return await jsRuntime.InvokeAsync<T>(identifier, args);
        }
        catch (JSException exception)
        {
            Debug.WriteLine($"JavaScript interop '{identifier}' failed: {exception.Message}");
            return default!;
        }
        catch (InvalidOperationException exception)
        {
            Debug.WriteLine($"JavaScript interop '{identifier}' is unavailable: {exception.Message}");
            return default!;
        }
    }

    public static async ValueTask<bool> InvokeVoidAsyncSafe(
        this IJSObjectReference jsModule,
        string identifier,
        params object?[]? args)
    {
        try
        {
            await jsModule.InvokeVoidAsync(identifier, args);
            return true;
        }
        catch (JSException exception)
        {
            Debug.WriteLine($"JavaScript module call '{identifier}' failed: {exception.Message}");
            return false;
        }
        catch (InvalidOperationException exception)
        {
            Debug.WriteLine($"JavaScript module call '{identifier}' is unavailable: {exception.Message}");
            return false;
        }
    }

    public static async ValueTask<T> InvokeAsyncSafe<T>(
        this IJSObjectReference jsModule,
        string identifier,
        params object?[]? args)
    {
        try
        {
            return await jsModule.InvokeAsync<T>(identifier, args);
        }
        catch (JSException exception)
        {
            Debug.WriteLine($"JavaScript module call '{identifier}' failed: {exception.Message}");
            return default!;
        }
        catch (InvalidOperationException exception)
        {
            Debug.WriteLine($"JavaScript module call '{identifier}' is unavailable: {exception.Message}");
            return default!;
        }
    }
#pragma warning restore BL0016
}
