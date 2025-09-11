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
                reject(new Error('bootstrap not available'));
                return;
            }
            setTimeout(check, delayMs);
        };
        check();
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
        // Ensure we focus and select the filter input after the modal is fully shown
        el.addEventListener('shown.bs.modal', () => {
            try { focusAndSelectFilter(selector); } catch (err) { console.error('selectionModalInterop: focus on shown failed', err); }
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
            console.log('selectionModalInterop: modal hidden', selector);
        } else {
            console.log('selectionModalInterop: no existing modal instance, creating then hiding');
            const created = new bs.Modal(el);
            created.hide();
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

