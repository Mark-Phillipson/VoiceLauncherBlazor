/* Reconnect bridge
   - Shows the existing #blazor-error-ui banner when the browser is offline or the server is unreachable.
   - Dispatches `components-reconnect-state-changed` events on the reconnect modal element
     so the existing ReconnectModal.razor.js can react (`show`, `hide`, `failed`, `rejected`).
   - Polls the server periodically to detect server process crashes even when navigator.onLine is true.
*/
(function () {
    // Debug helper to trace what's happening when testing in the browser console
    try { console.debug && console.debug('[reconnect-bridge] init'); } catch (e) { }

    function getErrorUi() { return document.getElementById('blazor-error-ui'); }
    function getReconnectModal() { return document.getElementById('components-reconnect-modal'); }

    function ensureHandlers() {
        const errorUi = getErrorUi();
        if (!errorUi) return;
        errorUi.querySelectorAll('.reload').forEach(el => {
            if (el.__reconnect_bridge) return;
            el.__reconnect_bridge = true;
            el.addEventListener('click', (e) => { e.preventDefault(); location.reload(); });
        });
        errorUi.querySelectorAll('.dismiss').forEach(el => {
            if (el.__reconnect_bridge) return;
            el.__reconnect_bridge = true;
            el.addEventListener('click', (e) => { e.preventDefault(); errorUi.style.display = 'none'; });
        });
    }

    function showErrorUIForOffline() {
        const errorUi = getErrorUi();
        if (!errorUi) return;
        ensureHandlers();
        let span = errorUi.querySelector('.reconnect-bridge-message');
        if (!span) {
            span = document.createElement('span');
            span.className = 'reconnect-bridge-message';
            span.style.marginRight = '1rem';
            span.style.fontWeight = '600';
            errorUi.insertBefore(span, errorUi.firstChild);
        }
        span.textContent = 'Offline — please refresh the page.';
        // Use a CSS class override and inline important style so site styles
        // (including compiled component CSS) don't accidentally keep the banner hidden.
        errorUi.classList.add('reconnect-bridge-show');
        try { errorUi.style.setProperty('display', 'block', 'important'); } catch (e) { }
        try { console.debug && console.debug('[reconnect-bridge] show error UI'); } catch (e) {}
    }

    function hideErrorUI() {
        const errorUi = getErrorUi();
        if (!errorUi) return;
        errorUi.classList.remove('reconnect-bridge-show');
        // Remove the inline display override so normal CSS rules can take over again
        try { errorUi.style.removeProperty('display'); } catch (e) { }
        const span = errorUi.querySelector('.reconnect-bridge-message');
        if (span) span.remove();
        try { console.debug && console.debug('[reconnect-bridge] hide error UI'); } catch (e) {}
    }

    function dispatchState(state) {
        try {
            const reconnectModal = getReconnectModal();
            if (!reconnectModal) return;
            const ev = new CustomEvent('components-reconnect-state-changed', { detail: { state } });
            reconnectModal.dispatchEvent(ev);
        } catch (e) {
            // noop
        }
    }

    window.addEventListener('offline', () => {
        showErrorUIForOffline();
        dispatchState('failed');
    });

    window.addEventListener('online', () => {
        hideErrorUI();
        dispatchState('hide');
    });

    // Listen for several possible Blazor lifecycle/connection events. Different runtime
    // versions emit slightly different event names; handle a broad set conservatively.
    const eventsMap = {
        'reconnecting': 'show',
        'blazor:reconnecting': 'show',
        'reconnected': 'hide',
        'blazor:reconnected': 'hide',
        'connectionDown': 'failed',
        'connectionUp': 'hide',
        'blazor:error': 'failed'
    };

    Object.keys(eventsMap).forEach(evtName => {
        try {
            document.addEventListener(evtName, () => {
                const desired = eventsMap[evtName];
                if (desired === 'show') {
                    dispatchState('show');
                    showErrorUIForOffline();
                } else if (desired === 'hide') {
                    dispatchState('hide');
                    hideErrorUI();
                } else if (desired === 'failed') {
                    dispatchState('failed');
                    showErrorUIForOffline();
                }
            });
        } catch (e) { }
    });

    // Poll the server to detect when the server process is down (connection refused),
    // even when the browser reports `navigator.onLine === true`.
    let serverDown = false;

    async function pollServer() {
        if (!('fetch' in window)) return;
        if (!navigator.onLine) return;
        const url = window.location.href.split('#')[0];
        try {
            const res = await fetch(url, { cache: 'no-store', method: 'GET', headers: { 'X-Reconnect-Bridge': '1' } });
            if (!res || !res.ok) {
                if (!serverDown) {
                    serverDown = true;
                    showErrorUIForOffline();
                    dispatchState('failed');
                }
            } else {
                if (serverDown) {
                    serverDown = false;
                    hideErrorUI();
                    dispatchState('hide');
                }
            }
        } catch (err) {
            if (!serverDown) {
                serverDown = true;
                showErrorUIForOffline();
                dispatchState('failed');
            }
        }
    }

    // Initial check after a short delay and then periodic polling
    setTimeout(() => {
        try {
            ensureHandlers();
            if (!navigator.onLine) {
                showErrorUIForOffline();
                dispatchState('failed');
            } else {
                pollServer();
            }
        } catch (e) { }
    }, 500);

    setInterval(pollServer, 15000);
})();
