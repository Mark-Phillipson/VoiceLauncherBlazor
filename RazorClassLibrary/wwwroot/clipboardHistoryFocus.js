let focusHandler;

export function register(dotNetReference) {
    if (focusHandler) {
        window.removeEventListener("focus", focusHandler);
    }

    let refreshPending = false;
    focusHandler = async () => {
        if (refreshPending) {
            return;
        }

        refreshPending = true;
        try {
            await dotNetReference.invokeMethodAsync("RefreshWhenWindowFocused");
        } catch {
            // Ignore callbacks after the Blazor component has been disposed.
        } finally {
            refreshPending = false;
        }
    };

    window.addEventListener("focus", focusHandler);
}

export function unregister() {
    if (!focusHandler) {
        return;
    }

    window.removeEventListener("focus", focusHandler);
    focusHandler = null;
}
