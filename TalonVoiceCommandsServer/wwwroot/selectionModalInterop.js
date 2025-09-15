function waitForBootstrap(retries = 10, delayMs = 100) {
    return new Promise((resolve, reject) => {
        let attempts = 0;
        const check = () => {
            attempts++;
            if (window.bootstrap && window.bootstrap.Modal) {
                console.log('selectionModalInterop: bootstrap available');
                resolve(window.bootstrap);
                return;
            }
            if (attempts >= retries) {
                // Try to dynamically load the local bootstrap bundle, then CDN as a fallback
                const tryLoad = async () => {
                    try {
                        await loadScript('/lib/bootstrap/dist/js/bootstrap.bundle.min.js');
                        // Give bootstrap a moment to initialize
                        setTimeout(() => {
                            if (window.bootstrap && window.bootstrap.Modal) {
                                resolve(window.bootstrap);
                            } else {
                                // fallback to CDN
                                loadScript('https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/js/bootstrap.bundle.min.js')
                                    .then(() => {
                                        setTimeout(() => {
                                            if (window.bootstrap && window.bootstrap.Modal) {
                                                resolve(window.bootstrap);
                                            } else {
                                                reject(new Error('bootstrap not available after loading scripts'));
                                            }
                                        }, 100);
                                    })
                                    .catch(() => reject(new Error('bootstrap not available and CDN load failed')));
                            }
                        }, 50);
                    } catch (e) {
                        // failed to load local, try CDN
                        try {
                            await loadScript('https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/js/bootstrap.bundle.min.js');
                            setTimeout(() => {
                                if (window.bootstrap && window.bootstrap.Modal) {
                                    resolve(window.bootstrap);
                                } else {
                                    reject(new Error('bootstrap not available after CDN load'));
                                }
                            }, 50);
                        } catch (err) {
                            reject(new Error('bootstrap not available'));
                        }
                    }
                };

                tryLoad();
                return;
            }
            setTimeout(check, delayMs);
        };
        check();
    });
}

// Helper to dynamically insert a script tag and return a promise that resolves on load
function loadScript(src) {
    return new Promise((resolve, reject) => {
        try {
            // If it's already present, resolve immediately
            const existing = Array.from(document.getElementsByTagName('script')).find(s => s.src && s.src.indexOf(src) !== -1);
            if (existing) {
                // if already loaded and ready, resolve quickly
                if (existing.hasAttribute('data-loaded')) {
                    resolve();
                    return;
                }
                existing.addEventListener('load', () => resolve());
                existing.addEventListener('error', () => reject(new Error('script load failed')));
                return;
            }

            const s = document.createElement('script');
            s.src = src;
            s.defer = true;
            s.async = false;
            s.onload = function () {
                s.setAttribute('data-loaded', '1');
                resolve();
            };
            s.onerror = function () { reject(new Error('script load failed: ' + src)); };
            document.head.appendChild(s);
        } catch (err) {
            reject(err);
        }
    });
}

async function showModal(selector) {
    console.log('selectionModalInterop.showModal called for', selector);
    const el = document.querySelector(selector);
    if (!el) {
        console.warn('selectionModalInterop: element not found for selector', selector);
        return;
    }
    try {
        const bs = window.bootstrap && window.bootstrap.Modal ? window.bootstrap : await waitForBootstrap();
        const modal = new bs.Modal(el);
        // Accessibility fix: set aria-hidden to false when modal is shown
        el.setAttribute('aria-hidden', 'false');
        // Ensure we focus and select the filter input after the modal is fully shown
        el.addEventListener('shown.bs.modal', () => {
            try { focusAndSelectFilter(selector); } catch (err) { console.error('selectionModalInterop: focus on shown failed', err); }
            el.setAttribute('aria-hidden', 'false');
        }, { once: true });
        // Also set aria-hidden to true when modal is hidden
        el.addEventListener('hidden.bs.modal', () => {
            el.setAttribute('aria-hidden', 'true');
        }, { once: true });
        modal.show();
        console.log('selectionModalInterop: modal shown', selector);
    } catch (err) {
        console.error('selectionModalInterop: showModal failed', err);
    }
}

// Focus and select the filter input inside the modal
async function focusAndSelectFilter(selector) {
    try {
        const root = document.querySelector(selector);
        if (!root) return;
        const input = root.querySelector('.selection-filter');
        if (!input) return;
        // Focus and select all content
        input.focus();
        if (typeof input.select === 'function') {
            input.select();
        } else if (input.setSelectionRange) {
            input.setSelectionRange(0, input.value.length);
        }
    } catch (err) {
        console.error('selectionModalInterop: focusAndSelectFilter failed', err);
    }
}

async function hideModal(selector) {
    console.log('selectionModalInterop.hideModal called for', selector);
    const el = document.querySelector(selector);
    if (!el) {
        console.warn('selectionModalInterop: element not found for selector', selector);
        return;
    }
    try {
        const bs = window.bootstrap && window.bootstrap.Modal ? window.bootstrap : await waitForBootstrap();
        const modal = bs.Modal.getInstance(el);
        if (modal) {
            modal.hide();
            el.setAttribute('aria-hidden', 'true'); // Accessibility fix
            console.log('selectionModalInterop: modal hidden', selector);
        } else {
            console.log('selectionModalInterop: no existing modal instance, creating then hiding');
            const created = new bs.Modal(el);
            created.hide();
            el.setAttribute('aria-hidden', 'true'); // Accessibility fix
        }
    } catch (err) {
        console.error('selectionModalInterop: hideModal failed', err);
    }
}

// Expose for legacy consumers
window.bootstrapInterop = {
    showModal: showModal,
    hideModal: hideModal,
    focusAndSelectFilter: focusAndSelectFilter
};

export { showModal, hideModal, focusAndSelectFilter };

// Provide a fallback for scrollFocusedIntoView used by some server-side components
if (!window.scrollFocusedIntoView) {
    window.scrollFocusedIntoView = function () {
        try {
            const focused = document.querySelector('.card.focused');
            if (focused && typeof focused.scrollIntoView === 'function') {
                focused.scrollIntoView({ behavior: 'smooth', block: 'center' });
            }
        } catch (err) {
            console.error('selectionModalInterop: scrollFocusedIntoView failed', err);
        }
    };
}
