window.blazorClipboard = {
    async getImageFromClipboard() {
        // Try modern clipboard.read() first (requires secure context and permission)
        try {
            if (navigator.clipboard && navigator.clipboard.read) {
                try {
                    const items = await navigator.clipboard.read();
                    for (const item of items) {
                        for (const type of item.types) {
                            if (type.startsWith('image/')) {
                                const blob = await item.getType(type);
                                const dataUrl = await blobToDataUrl(blob);
                                return dataUrl;
                            }
                        }
                    }
                } catch (e) {
                    // fall through to paste-event fallback
                    console.warn('clipboard.read failed, falling back to paste event:', e);
                }
            }
        } catch (ex) {
            console.warn('clipboard.read not available or failed:', ex);
        }

        // Fallback: wait for a user paste event (user must press Ctrl+V)
        return await awaitPasteEvent();
    },

    // Return a data URL found inside the element (e.g., pasted <img src="data:...">)
    getDataUrlFromElement(elementId) {
        try {
            const el = document.getElementById(elementId);
            if (!el) return null;
            const img = el.querySelector('img');
            if (img && img.src && img.src.indexOf('data:') === 0) return img.src;
            return null;
        } catch (err) { console.warn('getDataUrlFromElement error', err); return null; }
    },

    cancelAwaitPasteEvent() {
        cancelAwaitPasteEvent();
    },

    registerPasteHandler(elementId, dotNetHelper) {
        console.log('Registering paste handler for ' + elementId);
        const target = document.getElementById(elementId);
        if (!target) {
            console.warn('Target not found: ' + elementId);
            return;
        }

        target.addEventListener('paste', async (e) => {
            console.log('Paste event received');
            const items = e.clipboardData && e.clipboardData.items;
            if (items) {
                console.log('Items found: ' + items.length);
                for (let i = 0; i < items.length; i++) {
                    const item = items[i];
                    console.log('Item type: ' + item.type);
                    if (item.type && item.type.indexOf('image') !== -1) {
                        // We have an image in the clipboard. Prevent the default insertion and handle it.
                        try { e.preventDefault(); } catch (err) { /* ignore */ }
                        const blob = item.getAsFile();
                        const dataUrl = await blobToDataUrl(blob);
                        // Insert a visible image into the paste target so the user sees feedback
                        try {
                            const img = document.createElement('img');
                            img.src = dataUrl;
                            img.style.maxWidth = '100%';
                            img.style.maxHeight = '100%';
                            try { target.innerHTML = ''; } catch (err) { /* ignore */ }
                            try { target.appendChild(img); } catch (err) { console.warn('Failed to insert image into paste target', err); }
                        } catch (insertErr) { console.warn('Error inserting pasted image into target', insertErr); }

                        // Upload the blob to the server via HTTP POST to avoid sending large base64 over SignalR.
                        try {
                            const fd = new FormData();
                            fd.append('file', blob, 'pasted-image');
                            const resp = await fetch('/api/clipboard/upload', { method: 'POST', body: fd });
                            if (resp.ok) {
                                const json = await resp.json();
                                const token = json.token;
                                console.log('Invoking OnImageTokenReceived with token', token);
                                await dotNetHelper.invokeMethodAsync('OnImageTokenReceived', token);
                            } else {
                                console.warn('Image upload failed', resp.status);
                                // fallback to sending dataUrl via SignalR if needed
                                try { await dotNetHelper.invokeMethodAsync('OnImagePasted', dataUrl); } catch (e) { }
                            }
                        } catch (uploadErr) {
                            console.warn('Upload error', uploadErr);
                            try { await dotNetHelper.invokeMethodAsync('OnImagePasted', dataUrl); } catch (e) { }
                        }
                        return;
                    }
                }
                // No image item found; allow default paste behaviour so contenteditable receives inserted HTML
                console.log('Paste contained no image items; allowing default paste behavior');
            } else {
                console.log('No items in clipboardData; allowing default paste behaviour');
            }
        });
    },

    registerDropHandler(elementId, dotNetHelper) {
        console.log('Registering drop handler for ' + elementId);
        const target = document.getElementById(elementId);
        if (!target) {
            console.warn('Drop target not found: ' + elementId);
            return;
        }

        target.addEventListener('dragover', (e) => {
            e.preventDefault();
            e.dataTransfer.dropEffect = 'copy';
        });

        target.addEventListener('drop', async (e) => {
            e.preventDefault();
            try {
                const files = e.dataTransfer && e.dataTransfer.files;
                if (files && files.length > 0) {
                    for (let i = 0; i < files.length; i++) {
                        const f = files[i];
                        if (f.type && f.type.indexOf('image') !== -1) {
                                const dataUrl = await blobToDataUrl(f);
                                // Insert visible image into the drop target
                                try {
                                    const img = document.createElement('img');
                                    img.src = dataUrl;
                                    img.style.maxWidth = '100%';
                                    img.style.maxHeight = '100%';
                                    try { target.innerHTML = ''; } catch (err) { /* ignore */ }
                                    try { target.appendChild(img); } catch (err) { console.warn('Failed to insert dropped image into drop target', err); }
                                } catch (insertErr) { console.warn('Error inserting dropped image into target', insertErr); }
                                try { await dotNetHelper.invokeMethodAsync('OnImagePasted', dataUrl); } catch (err) { console.warn(err); }
                                return;
                            }
                    }
                }
            } catch (err) { console.warn('drop handler error', err); }
        });
    }
};

function blobToDataUrl(blob) {
    return new Promise((resolve, reject) => {
        const reader = new FileReader();
        reader.onloadend = function () {
            resolve(reader.result);
        };
        reader.onerror = reject;
        reader.readAsDataURL(blob);
    });
}

function awaitPasteEvent() {
    return new Promise((resolve) => {
        // store resolve so cancel can call it
        pendingResolve = resolve;

        // Prefer existing element with id 'blazorPasteCatcher' (visible in UI), otherwise create a hidden one
        let pasteCatcher = document.getElementById('blazorPasteCatcher');
        let createdByScript = false;
        if (!pasteCatcher) {
            createdByScript = true;
            pasteCatcher = document.createElement('div');
            pasteCatcher.setAttribute('contenteditable', 'true');
            pasteCatcher.style.position = 'fixed';
            pasteCatcher.style.left = '-9999px';
            pasteCatcher.style.width = '1px';
            pasteCatcher.style.height = '1px';
            pasteCatcher.id = 'blazorPasteCatcher';
            // Mark generated element so we know whether to remove it later
            pasteCatcher.setAttribute('data-generated', 'true');
            document.body.appendChild(pasteCatcher);
        }
        // store ref to allow cancel to clear it
        pendingPasteCatcher = pasteCatcher;
        try { pasteCatcher.focus(); } catch { }

        const handler = async (e) => {
            try {
                const items = e.clipboardData && e.clipboardData.items;
                if (items) {
                    for (let i = 0; i < items.length; i++) {
                        const item = items[i];
                        if (item.type && item.type.indexOf('image') !== -1) {
                            const blob = item.getAsFile();
                            const dataUrl = await blobToDataUrl(blob);
                            cleanup();
                            resolve(dataUrl);
                            return;
                        }
                    }
                }

                // Some browsers may paste as HTML with an <img> element into the contenteditable
                try {
                    const img = pasteCatcher.querySelector('img');
                    if (img && img.src && img.src.indexOf('data:') === 0) {
                        const dataUrl = img.src;
                        cleanup();
                        resolve(dataUrl);
                        return;
                    }
                } catch (err) { /* ignore */ }
            } catch (err) {
                console.warn('paste handler error', err);
            }
            cleanup();
            resolve(null);
        };

        const cleanup = () => {
            try { pasteCatcher.removeEventListener('paste', handler); } catch { }
            // Only remove the element if it was created by the JS helper, not when it's in the UI pre-existing
            try { if (pasteCatcher && pasteCatcher.id === 'blazorPasteCatcher' && pasteCatcher.getAttribute('data-generated') === 'true') { pasteCatcher.parentNode && pasteCatcher.parentNode.removeChild(pasteCatcher); } } catch { }
            pendingPasteCatcher = null;
            if (pendingTimeout) { clearTimeout(pendingTimeout); pendingTimeout = null; }
            pendingResolve = null;
        };

            pasteCatcher.addEventListener('paste', handler, { once: true });

        // Timeout after 10 seconds
        pendingTimeout = setTimeout(() => {
            try { pasteCatcher.removeEventListener('paste', handler); } catch { }
            try { if (pasteCatcher && pasteCatcher.getAttribute('data-generated') === 'true') { pasteCatcher.parentNode && pasteCatcher.parentNode.removeChild(pasteCatcher); } } catch { }
            pendingPasteCatcher = null;
            if (pendingResolve) { pendingResolve(null); pendingResolve = null; }
        }, 10000);
    });
}

let pendingResolve = null;
let pendingTimeout = null;
let pendingPasteCatcher = null;

function cancelAwaitPasteEvent() {
    if (pendingResolve) {
        try { pendingResolve(null); } catch { }
        pendingResolve = null;
    }
    if (pendingTimeout) {
        clearTimeout(pendingTimeout);
        pendingTimeout = null;
    }
    if (pendingPasteCatcher) {
        try { pendingPasteCatcher.parentNode && pendingPasteCatcher.parentNode.removeChild(pendingPasteCatcher); } catch { }
        pendingPasteCatcher = null;
    }
}
