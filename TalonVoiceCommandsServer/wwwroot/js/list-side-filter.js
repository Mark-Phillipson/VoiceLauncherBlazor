window.listSideFilter = (function () {
    // Attach client-side filter to the input inside the panel.
    function attach(panelSelector, inputSelector, rowsSelector) {
        try {
            const panel = document.querySelector(panelSelector);
            if (!panel) return;
            const input = panel.querySelector(inputSelector) || document.querySelector(inputSelector);
            if (!input) return;
            // Remove existing handler if any
            if (input._listSideFilterHandler) {
                input.removeEventListener('input', input._listSideFilterHandler);
                input._listSideFilterHandler = null;
            }

            const handler = function (ev) {
                const term = (ev.target.value || '').trim().toLowerCase();
                const rows = panel.querySelectorAll(rowsSelector);
                rows.forEach(r => {
                    const text = (r.innerText || '').toLowerCase();
                    if (!term) {
                        r.style.display = '';
                    } else {
                        r.style.display = text.indexOf(term) >= 0 ? '' : 'none';
                    }
                });
            };

            input.addEventListener('input', handler);
            input._listSideFilterHandler = handler;
        }
        catch (e) { console.error('listSideFilter.attach error', e); }
    }

    function detach(panelSelector, inputSelector) {
        try {
            const panel = document.querySelector(panelSelector);
            if (!panel) return;
            const input = panel.querySelector(inputSelector) || document.querySelector(inputSelector);
            if (!input) return;
            if (input._listSideFilterHandler) {
                input.removeEventListener('input', input._listSideFilterHandler);
                input._listSideFilterHandler = null;
            }
            // Optionally, show all rows again
            const rows = panel.querySelectorAll('.table-scroll-table tbody tr');
            rows.forEach(r => r.style.display = '');
        }
        catch (e) { console.error('listSideFilter.detach error', e); }
    }

    return { attach, detach };
})();
